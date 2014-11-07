var ros = ros || {};

var Connection = function(url) {
  this.handlers = new Array();

  if (typeof WebSocket == 'undefined') {
    WebSocket = MozWebSocket;
  }
  this.socket = new WebSocket(url);
  this.onmessage = null;
  var ths = this;
  this.socket.onmessage = function(e) {
    if(ths.onmessage) {
      try {
        ths.onmessage(e);
      } catch(err) {
        ros_debug(err);
      }
    }

    var call = ''; 
    try {
      eval('call = ' + e.data);
    } catch(err) {
      return;
    }

    for (var i in ths.handlers[call.receiver]) {
      var handler = ths.handlers[call.receiver][i]
      handler(call.msg);
    }
  }

  this.magicServices = new Array('/rosbridge/topics','/rosbridge/services','/rosbridge/typeStringFromTopic','/rosbridge/typeStringFromService','/rosbridge/msgClassFromTypeString','/rosbridge/reqClassFromTypeString','/rosbridge/rspClassFromTypeString','/rosbridge/classFromTopic','/rosbridge/classesFromService');

}

Connection.prototype.callService = function(service, json, callback) {
  this.handlers[service] = new Array(callback);
  var call = '{"receiver":"' + service + '"';
  call += ',"msg":' + json + '}';
  this.socket.send(call);
}

Connection.prototype.publish = function(topic, typeStr, json) {
  typeStr.replace(/^\//,'');
  var call = '{"receiver":"' + topic + '"';
  call += ',"msg":' + json;
  call += ',"type":"' + typeStr + '"}';
  this.socket.send(call);
}

Connection.prototype.addHandler = function(topic, func) {
  if (!(topic in this.handlers)) {
    this.handlers[topic] = new Array();
  }
  this.handlers[topic].push(func);
}

Connection.prototype.setOnError = function(func) {
  this.socket.onerror = func;
}

Connection.prototype.setOnClose = function(func) {
  this.socket.onclose = func;
}

Connection.prototype.setOnOpen = function(func) {
  this.socket.onopen = func;
}

Connection.prototype.setOnMessage = function(func) {
  this.onmessage = func;
}

ros.Connection = Connection;