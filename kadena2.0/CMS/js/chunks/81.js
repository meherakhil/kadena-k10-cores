webpackJsonp([81],{409:function(module,exports,__webpack_require__){try{(function(){"use strict";function _classCallCheck(instance,Constructor){if(!(instance instanceof Constructor))throw new TypeError("Cannot call a class as a function")}Object.defineProperty(exports,"__esModule",{value:!0});var _createClass=function(){function defineProperties(target,props){for(var i=0;i<props.length;i++){var descriptor=props[i];descriptor.enumerable=descriptor.enumerable||!1,descriptor.configurable=!0,"value"in descriptor&&(descriptor.writable=!0),Object.defineProperty(target,descriptor.key,descriptor)}}return function(Constructor,protoProps,staticProps){return protoProps&&defineProperties(Constructor.prototype,protoProps),staticProps&&defineProperties(Constructor,staticProps),Constructor}}(),Pagination=function(){function Pagination(wrapper,data){_classCallCheck(this,Pagination),this.code="",this.extend(data),this.create(wrapper),this.start()}return _createClass(Pagination,[{key:"extend",value:function(data){data=data||{},this.size=data.size||300,this.page=data.page||1,this.step=data.step||3,this.callback=data.callback,this.container=data.container,this.toPage=data.toPage,this.fromPage=data.fromPage,this.rowsOnPage=data.rowsOnPage}},{key:"add",value:function(s,f){for(var i=s;i<f;i+=1)this.code+='<p data-page-number="'+i+'">'+i+"</p>"}},{key:"last",value:function(){this.code+="<i>...</i><p> "+this.size+"</p>"}},{key:"first",value:function(){this.code+="<p>1</p><i>...</i>"}},{key:"click",value:function(event){this.prevPage=this.page,this.page=+event.target.innerHTML,this.start()}},{key:"prev",value:function(){this.prevPage=this.page,this.page=this.page<=1?1:this.page-1,this.start()}},{key:"next",value:function(){this.prevPage=this.page,this.page=this.page>=this.size?this.size:this.page+1,this.start()}},{key:"bind",value:function(){for(var _this=this,a=this.wrapper.getElementsByTagName("p"),i=0;i<a.length;i+=1)+a[i].innerHTML===this.page&&(a[i].className="generated-paginator__current"),a[i].addEventListener("click",function(event){return _this.click.call(_this,event)},!1);this.callback(this.prevPage,this.page,this.container,this.toPage,this.fromPage,this.rowsOnPage)}},{key:"finish",value:function(){this.wrapper.innerHTML=this.code,this.code="",this.bind()}},{key:"start",value:function(){this.size<2*this.step+6?this.add(1,this.size+1):this.page<2*this.step+1?(this.add(1,2*this.step+4),this.last()):this.page>this.size-2*this.step?(this.first(),this.add(this.size-2*this.step-2,this.size+1)):(this.first(),this.add(this.page-this.step,this.page+this.step+1),this.last()),this.finish()}},{key:"buttons",value:function(wrapper){var nav=wrapper.getElementsByTagName("p");nav[0].addEventListener("click",this.prev.bind(this),!1),nav[1].addEventListener("click",this.next.bind(this),!1)}},{key:"create",value:function(wrapper){var html=['<p class="generated-paginator__prev">&#60; Previous</p>',"<span></span>",'<p class="generated-paginator__next">Next &#62;</p>'];wrapper.innerHTML=html.join(""),this.wrapper=wrapper.getElementsByTagName("span")[0],this.buttons(wrapper)}}]),Pagination}();exports.default=Pagination}).call(this)}finally{}}});