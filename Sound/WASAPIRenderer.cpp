// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved
//
//#include "StdAfx.h"
//#include <assert.h>
//#include <avrt.h>
//#include "WASAPIRenderer.h"
//
//#include <mmsystem.h>


#define _CRT_SECURE_CPP_OVERLOAD_SECURE_NAMES 1
#include <new>
#include <windows.h>
#include <strsafe.h>
#include <objbase.h>
#pragma warning(push)
#pragma warning(disable : 4201)
#include <mmdeviceapi.h>
#include <audiopolicy.h>
#pragma warning(pop)


#include <assert.h>
#include <avrt.h>
#include "WASAPIRenderer.h"

bool DisableMMCSS;
//
//  A simple WASAPI Render client.
//

template <class T> void SafeRelease(T **ppT)
{
    if (*ppT)
    {
        (*ppT)->Release();
        *ppT = NULL;
    }
}

CWASAPIRenderer::CWASAPIRenderer(IMMDevice *Endpoint, int stereoPair) : 
    _RefCount(1),
    _Endpoint(Endpoint),
    _AudioClient(NULL),
    _RenderClient(NULL),
    _RenderThread(NULL),
    _ShutdownEvent(NULL),
    _MixFormat(NULL),
    _RenderBufferQueue(0),
	_stereoPair(stereoPair)
{
    _Endpoint->AddRef();    // Since we're holding a copy of the endpoint, take a reference to it.  It'll be released in Shutdown();
}

//
//  Empty destructor - everything should be released in the Shutdown() call.
//
CWASAPIRenderer::~CWASAPIRenderer(void) 
{
}
#define PERIODS_PER_BUFFER 4
//
//  Initialize WASAPI in event driven mode, associate the audio client with our samples ready event handle, and retrieve 
//  a render client for the transport.
//
bool CWASAPIRenderer::InitializeAudioEngine()
{
    REFERENCE_TIME bufferDuration = _EngineLatencyInMS*10000*PERIODS_PER_BUFFER;
    REFERENCE_TIME periodicity = _EngineLatencyInMS*10000;

    //
    //  We initialize the engine with a periodicity of _EngineLatencyInMS and a buffer size of PERIODS_PER_BUFFER times the latency - this ensures 
    //  that we will always have space available for rendering audio.  We only need to do this for exclusive mode timer driven rendering.
    //
    HRESULT hr = _AudioClient->Initialize(AUDCLNT_SHAREMODE_EXCLUSIVE, 
        AUDCLNT_STREAMFLAGS_NOPERSIST, 
        bufferDuration, 
        periodicity,
        _MixFormat, 
        NULL);
    if (FAILED(hr))
    {
        TRACE("Unable to initialize audio client: %x.\n", hr);
        return false;
    }

    //
    //  Retrieve the buffer size for the audio client.
    //
    hr = _AudioClient->GetBufferSize(&_BufferSize);
    if(FAILED(hr))
    {
        TRACE("Unable to get audio client buffer: %x. \n", hr);
        return false;
    }

    hr = _AudioClient->GetService(IID_PPV_ARGS(&_RenderClient));
    if (FAILED(hr))
    {
        TRACE("Unable to get new render client: %x.\n", hr);
        return false;
    }

    return true;
}
//
//  That buffer duration is calculated as being PERIODS_PER_BUFFER x the
//  periodicity, so each period we're going to see 1/PERIODS_PER_BUFFERth 
//  the size of the buffer.
//
UINT32 CWASAPIRenderer::BufferSizePerPeriod()
{
    return _BufferSize / PERIODS_PER_BUFFER;
}

//
//  Retrieve the format we'll use to rendersamples.
//
//  Start with the mix format and see if the endpoint can render that.  If not, try
//  the mix format converted to an integer form (most audio solutions don't support floating 
//  point rendering and the mix format is usually a floating point format).
//
bool CWASAPIRenderer::LoadFormat()
{
    HRESULT hr = _AudioClient->GetMixFormat(&_MixFormat);
    if (FAILED(hr))
    {
        TRACE("Unable to get mix format on audio client: %x.\n", hr);
        return false;
    }
    assert(_MixFormat != NULL);

	if(_stereoPair==0){
		_MixFormat->nChannels = 26;
	}
	TRACE("Find %i chan device...",_MixFormat->nChannels);

    hr = _AudioClient->IsFormatSupported(AUDCLNT_SHAREMODE_EXCLUSIVE,_MixFormat, NULL);
    if (hr == AUDCLNT_E_UNSUPPORTED_FORMAT)
    {
        TRACE("Device does not natively support the mix format, converting to PCM.\n");

        //
        //  If the mix format is a float format, just try to convert the format to PCM.
        //
        if (_MixFormat->wFormatTag == WAVE_FORMAT_IEEE_FLOAT)
        {
            _MixFormat->wFormatTag = WAVE_FORMAT_PCM;
            _MixFormat->wBitsPerSample = 16;
            _MixFormat->nBlockAlign = (_MixFormat->wBitsPerSample / 8) * _MixFormat->nChannels;
            _MixFormat->nAvgBytesPerSec = _MixFormat->nSamplesPerSec*_MixFormat->nBlockAlign;
        }
        else if (_MixFormat->wFormatTag == WAVE_FORMAT_EXTENSIBLE && 
            reinterpret_cast<WAVEFORMATEXTENSIBLE *>(_MixFormat)->SubFormat == KSDATAFORMAT_SUBTYPE_IEEE_FLOAT)
        {
            WAVEFORMATEXTENSIBLE *waveFormatExtensible = reinterpret_cast<WAVEFORMATEXTENSIBLE *>(_MixFormat);
            waveFormatExtensible->SubFormat = KSDATAFORMAT_SUBTYPE_PCM;
            waveFormatExtensible->Format.wBitsPerSample = 16;
            waveFormatExtensible->Format.nBlockAlign = (_MixFormat->wBitsPerSample / 8) * _MixFormat->nChannels;
			waveFormatExtensible->Format.nSamplesPerSec = 48000;
            waveFormatExtensible->Format.nAvgBytesPerSec = waveFormatExtensible->Format.nSamplesPerSec*waveFormatExtensible->Format.nBlockAlign;
            waveFormatExtensible->Samples.wValidBitsPerSample = 16;
        }
        else
        {
            TRACE("Mix format is not a floating point format.\n");
            return false;
        }

        hr = _AudioClient->IsFormatSupported(AUDCLNT_SHAREMODE_EXCLUSIVE,_MixFormat,NULL);
        if (FAILED(hr))
        {
            TRACE("Format is not supported \n");
            return false;
        }
    }

    _FrameSize = _MixFormat->nBlockAlign;
    if (!CalculateMixFormatType())
    {
        return false;
    }
    return true;
}

//
//  Crack open the mix format and determine what kind of samples are being rendered.
//
bool CWASAPIRenderer::CalculateMixFormatType()
{
    if (_MixFormat->wFormatTag == WAVE_FORMAT_PCM || 
        _MixFormat->wFormatTag == WAVE_FORMAT_EXTENSIBLE &&
            reinterpret_cast<WAVEFORMATEXTENSIBLE *>(_MixFormat)->SubFormat == KSDATAFORMAT_SUBTYPE_PCM)
    {
        if (_MixFormat->wBitsPerSample == 16)
        {
            _RenderSampleType = SampleType16BitPCM;
        }
        else
        {
            TRACE("Unknown PCM integer sample type\n");
            return false;
        }
    }
    else if (_MixFormat->wFormatTag == WAVE_FORMAT_IEEE_FLOAT ||
             (_MixFormat->wFormatTag == WAVE_FORMAT_EXTENSIBLE &&
               reinterpret_cast<WAVEFORMATEXTENSIBLE *>(_MixFormat)->SubFormat == KSDATAFORMAT_SUBTYPE_IEEE_FLOAT))
    {
        _RenderSampleType = SampleTypeFloat;
    }
    else 
    {
        TRACE("unrecognized device format.\n");
        return false;
    }
    return true;
}
//
//  Initialize the renderer.
//
bool CWASAPIRenderer::Initialize(UINT32 EngineLatency)
{
    //
    //  Create our shutdown and samples ready events- we want auto reset events that start in the not-signaled state.
    //
    _ShutdownEvent = CreateEventEx(NULL, NULL, 0, EVENT_MODIFY_STATE | SYNCHRONIZE);
    if (_ShutdownEvent == NULL)
    {
        TRACE("Unable to create shutdown event: %d.\n", GetLastError());
        return false;
    }


    //
    //  Now activate an IAudioClient object on our preferred endpoint and retrieve the mix format for that endpoint.
    //
    HRESULT hr = _Endpoint->Activate(__uuidof(IAudioClient), CLSCTX_INPROC_SERVER, NULL, reinterpret_cast<void **>(&_AudioClient));
    if (FAILED(hr))
    {
        TRACE("Unable to activate audio client: %x.\n", hr);
        return false;
    }

    //
    // Load the MixFormat.  This may differ depending on the shared mode used
    //
    if (!LoadFormat())
    {
        TRACE("Failed to load the mix format \n");
        return false;
    }

    //
    //  Remember our configured latency in case we'll need it for a stream switch later.
    //
    _EngineLatencyInMS = EngineLatency;

    if (!InitializeAudioEngine())
    {
        return false;
    }

    return true;
}

//
//  Shut down the render code and free all the resources.
//
void CWASAPIRenderer::Shutdown()
{
    if (_RenderThread)
    {
        SetEvent(_ShutdownEvent);
        WaitForSingleObject(_RenderThread, INFINITE);
        CloseHandle(_RenderThread);
        _RenderThread = NULL;
    }

    if (_ShutdownEvent)
    {
        CloseHandle(_ShutdownEvent);
        _ShutdownEvent = NULL;
    }

    SafeRelease(&_Endpoint);
    SafeRelease(&_AudioClient);
    SafeRelease(&_RenderClient);

    if (_MixFormat)
    {
        CoTaskMemFree(_MixFormat);
        _MixFormat = NULL;
    }
}


//
//  Start rendering - Create the render thread and start rendering the buffer.
//
bool CWASAPIRenderer::Start()
{
    HRESULT hr;

    _RenderBufferQueue = NULL;

    //
    //  We want to pre-roll one buffer's worth of silence into the pipeline.  That way the audio engine won't glitch on startup.  
    //  We pre-roll silence instead of audio buffers because our buffer size is significantly smaller than the engine latency 
    //  and we can only pre-roll one buffer's worth of audio samples.
    //  
    //
    {
        BYTE *pData;
        hr = _RenderClient->GetBuffer(_BufferSize, &pData);
        if (FAILED(hr))
        {
            TRACE("Failed to get buffer: %x.\n", hr);
            return false;
        }
        hr = _RenderClient->ReleaseBuffer(_BufferSize, AUDCLNT_BUFFERFLAGS_SILENT);
        if (FAILED(hr))
        {
            TRACE("Failed to release buffer: %x.\n", hr);
            return false;
        }
    }
    //
    //  Now create the thread which is going to drive the renderer.
    //
    _RenderThread = CreateThread(NULL, 0, WASAPIRenderThread, this, 0, NULL);
    if (_RenderThread == NULL)
    {
        TRACE("Unable to create transport thread: %x.", GetLastError());
        return false;
    }

    //
    //  We're ready to go, start rendering!
    //
    hr = _AudioClient->Start();
    if (FAILED(hr))
    {
        TRACE("Unable to start render client: %x.\n", hr);
        return false;
    }

    return true;
}
//
//  Stop the renderer.
//
void CWASAPIRenderer::Stop()
{
    HRESULT hr;

    //
    //  Tell the render thread to shut down, wait for the thread to complete then clean up all the stuff we 
    //  allocated in Start().
    //
    if (_ShutdownEvent)
    {
        SetEvent(_ShutdownEvent);
    }

    hr = _AudioClient->Stop();
    if (FAILED(hr))
    {
        TRACE("Unable to stop audio client: %x\n", hr);
    }

    if (_RenderThread)
    {
        WaitForSingleObject(_RenderThread, INFINITE);

        CloseHandle(_RenderThread);
        _RenderThread = NULL;
    }

    //
    //  Drain the buffers in the render buffer queue.
    //
    while (_RenderBufferQueue != NULL)
    {
        RenderBuffer *renderBuffer = _RenderBufferQueue;
        _RenderBufferQueue = renderBuffer->_Next;
        delete renderBuffer;
    }


}

//
//  Render thread - processes samples from the audio engine
//
DWORD CWASAPIRenderer::WASAPIRenderThread(LPVOID Context)
{
    CWASAPIRenderer *renderer = static_cast<CWASAPIRenderer *>(Context);
    return renderer->DoRenderThread();
}

DWORD CWASAPIRenderer::DoRenderThread()
{
    bool stillPlaying = true;
    HANDLE waitArray[1] = {_ShutdownEvent};
    HANDLE mmcssHandle = NULL;
    DWORD mmcssTaskIndex = 0;
	DWORD debugSamp = 0;
	DWORD debugSamp2 = 0;

	TRACE("Starting CWASAPIRenderer::DoRenderThread()");

    HRESULT hr = CoInitializeEx(NULL, COINIT_MULTITHREADED);
    if (FAILED(hr))
    {
        TRACE("Unable to initialize COM in render thread: %x\n", hr);
        return hr;
    }

    //
    //  We want to make sure that our timer resolution is a multiple of the latency, otherwise the system timer cadence will
    //  cause us to starve the renderer.
    //
    //  Set the system timer to 1ms as a worst case value.
    //
    timeBeginPeriod(1);

    if (!DisableMMCSS)
    {
        mmcssHandle = AvSetMmThreadCharacteristics(L"Audio", &mmcssTaskIndex);
        if (mmcssHandle == NULL)
        {
            TRACE("Unable to enable MMCSS on render thread: %d\n", GetLastError());
        }
    }

    while (stillPlaying)
    {
		//if(_stereoPair!=2)break;
        HRESULT hr;
		
        //
        //  When running in timer mode, wait for half the configured latency.
        //
        DWORD waitResult = WaitForMultipleObjects(1, waitArray, FALSE, _EngineLatencyInMS/2);
        switch (waitResult)
        {
        case WAIT_OBJECT_0 + 0:     // _ShutdownEvent
            stillPlaying = false;       // We're done, exit the loop.
            break;
        case WAIT_TIMEOUT:          // Timeout
    
            BYTE *pData;
            UINT32 padding;
            UINT32 framesAvailable;

            //
            //  We want to find out how much of the buffer *isn't* available (is padding).
            //
            hr = _AudioClient->GetCurrentPadding(&padding);
            if (SUCCEEDED(hr))
            {
                //
                //  Calculate the number of frames available.  We'll render
                //  that many frames or the number of frames left in the buffer, whichever is smaller.
                //
                framesAvailable = _BufferSize - padding;// - 500;

				BYTE buffer[100000]; 
				BYTE buffer2[100000]; 

				if(_stereoPair == -1){
					//just a regular pair
					_getSampleCallback((void*)&buffer[0],framesAvailable*_FrameSize,_stereoPair);
				}
				if(_stereoPair == 0){
					//8chan device
					//_FrameSize is 52
					//Actual device in use is 26 chan -- we use first 8

					short* b = (short*)&buffer[0];
					short* b2 = (short*)&buffer2[0];
					for(int pair=0;pair<4;pair++){
						_getSampleCallback((void*)&buffer2[0],framesAvailable*4,pair);
						for(int i=0;i<framesAvailable;i++){
							b[26*i+(2*pair)]=b2[2*i];
							b[26*i+(2*pair)+1]=b2[2*i+1];
						}
					}
				}
//				

				//short* b = (short*)&buffer[0];
				//for(int i=0;i<framesAvailable/**_FrameSize/4*/;i++){ 
				//	b[(_FrameSize/2)*i]=(short)(((double)0x39FF)*sin(((double)debugSamp++)/22.0) + 
				//		((double)0x39FF)*sin(1.5*((double)debugSamp2++)/22.0));
				//	b[(_FrameSize/2)*i+1]=b[2*i];
				//}

				//TRACE("FrameSize: %i",_FrameSize);
				//continue;

				//short* b2 = (short*)&buffer2[0];
				//for(int i=0;i<framesAvailable;i++){ 
				//	b2[i]=(short)(((double)0x39FF)*sin(((double)debugSamp++)/22.0) + 
				//		((double)0x39FF)*sin(1.5*((double)debugSamp2++)/22.0));
				//}
				//for(int i=0;i<framesAvailable;i++){
				//	b[8*i]=b2[i];
				//	b[8*i+1]=b2[i];
				//}

				hr = _RenderClient->GetBuffer(framesAvailable, &pData);
                if (SUCCEEDED(hr))
                {
                    //
                    //  Copy data from the render buffer to the output buffer and bump our render pointer.
                    //
                    CopyMemory(pData, buffer, framesAvailable*_FrameSize);
                    hr = _RenderClient->ReleaseBuffer(framesAvailable, 0);
                    if (!SUCCEEDED(hr))
                    {
                        TRACE("Unable to release buffer: %x\n", hr);
                        stillPlaying = false;
                    }
				} else {
					int i=0;
					i++;
				}
            }
            break;
        }
    }

    //
    //  Unhook from MMCSS.
    //
    if (!DisableMMCSS)
    {
        AvRevertMmThreadCharacteristics(mmcssHandle);
    }

    //
    //  Revert the system timer to the previous value.
    //
    timeEndPeriod(1);

    CoUninitialize();
    return 0;
}



//
//  IUnknown
//
HRESULT CWASAPIRenderer::QueryInterface(REFIID Iid, void **Object)
{
    if (Object == NULL)
    {
        return E_POINTER;
    }
    *Object = NULL;

    if (Iid == IID_IUnknown)
    {
        *Object = static_cast<IUnknown *>(this);
        AddRef();
    }
    else
    {
        return E_NOINTERFACE;
    }
    return S_OK;
}
ULONG CWASAPIRenderer::AddRef()
{
    return InterlockedIncrement(&_RefCount);
}
ULONG CWASAPIRenderer::Release()
{
    ULONG returnValue = InterlockedDecrement(&_RefCount);
    if (returnValue == 0)
    {
        delete this;
    }
    return returnValue;
}
