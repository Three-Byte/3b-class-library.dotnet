function Subscription (c, url, callback){
	this.subChannel = c;
	this.id = 0;
	this.currentData;
	this.callback = callback;
	this.subURL = url;

};

Subscription.prototype.init = function() {
    this.postRequest(this.id);
};

Subscription.prototype.parseResponse = function(data) {

    if (navigator.userAgent.indexOf("Firefox") > -1) {
        this.currentData = $.parseJSON(data);
    } else {
        this.currentData = data;
    }

    //Look at the id of the data
    var maxId = 0;
    for (var i = 0, len = this.currentData.Elements.length; i < len; i++) {
        var currentId = this.currentData.Elements[i].MessageID;

        maxId = Math.max(currentId, maxId);
    }

    this.id = maxId + 1;

    this.postRequest();
    this.callback(this.currentData);
    
};

Subscription.prototype.postRequest = function () {
    var theData = { 'Channels': [{ 'ID': this.subChannel, 'From': this.id}] };

    var jsonData = JSON.stringify(theData);

    var sub = this;

    var response = $.ajax({
        url: sub.subURL,
        type: "POST",
        headers: { "Content-Type": "application/json" },
        data: jsonData,
        processData: false,
        dataType: "json",
        success: function (msg) {
            sub.parseResponse(msg);
        }
    });

    response.fail(function (jqXHR, textStatus) {
        sub.postRequest();
        console.log("failed: " + textStatus);
    });
};

function Publication(url, successCallback, failCallback) {
    this.pubURL = url;
    this.successCallback = successCallback;
    this.failCallback = failCallback;
};

Publication.prototype.publish = function (element) {
    var pub = this;

    var jsonData = JSON.stringify(element);

    var response = $.ajax({
        url: pubURL,
        type: "POST",
        contentType: "application/json",
        data: jsonData,
        dataType: "json",
        success: function (msg) {
            if (msg.Status.indexOf("Error") > -1) {
                pub.failCallback(JSON.stringify(msg));
            } else {
                pub.successCallback(JSON.stringify(msg));
            }
        },
        error: function (msg) {
            pub.failCallback(JSON.stringify(msg));
        }
    });


};

