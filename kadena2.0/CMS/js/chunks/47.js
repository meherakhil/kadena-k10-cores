webpackJsonp([47],{386:function(module,exports,__webpack_require__){try{(function(){"use strict";function _defineProperty(obj,key,value){return key in obj?Object.defineProperty(obj,key,{value:value,enumerable:!0,configurable:!0,writable:!0}):obj[key]=value,obj}function _classCallCheck(instance,Constructor){if(!(instance instanceof Constructor))throw new TypeError("Cannot call a class as a function")}Object.defineProperty(exports,"__esModule",{value:!0});var _createClass=function(){function defineProperties(target,props){for(var i=0;i<props.length;i++){var descriptor=props[i];descriptor.enumerable=descriptor.enumerable||!1,descriptor.configurable=!0,"value"in descriptor&&(descriptor.writable=!0),Object.defineProperty(target,descriptor.key,descriptor)}}return function(Constructor,protoProps,staticProps){return protoProps&&defineProperties(Constructor.prototype,protoProps),staticProps&&defineProperties(Constructor,staticProps),Constructor}}(),Dropzone=function(){function Dropzone(container){_classCallCheck(this,Dropzone),this.container=container;var input=container.querySelector(".js-drop-zone-file"),item=container.querySelector(".js-drop-zone-item");this.fileContainer=container.querySelector(".js-drop-zone-droppped"),this.inputFilesCount=container.querySelector(".js-drop-zone-files-count"),this.nameInput=document.querySelector(".js-drop-zone-name-input"),this.acceptedFormatsStr=container.dataset.accepted,this.acceptedFormats=this.acceptedFormatsStr?this.acceptedFormatsStr.split(","):[],this.selector="isDropped",this.reverseSelector="isNotDropped",this.maxItems=container.dataset.maxItems?+container.dataset.maxItems:1e7,this.idealItem=item.cloneNode(!0),this.idealInput=input.cloneNode(!0),this.count=0,this.number=0,input.addEventListener("change",this.addFile.bind(this)),this.createRemover(item),this.data=_defineProperty({},this.number,{input:input,item:item}),document.querySelector(".js-drop-zone-submit").addEventListener("click",this.submit.bind(this))}return _createClass(Dropzone,[{key:"submit",value:function(){this.inputFilesCount.setAttribute("value",this.count);var inputs=this.container.querySelectorAll(".js-drop-zone-file"),index=1;inputs.forEach(function(input){input.value&&(input.setAttribute("name","file"+index),index+=1)})}},{key:"addFile",value:function(event){var _this=this,_Dropzone$getFileFull=Dropzone.getFileFullName(event.target.files[0]),name=_Dropzone$getFileFull.name,ext=_Dropzone$getFileFull.ext;if(!this.isFormatAccepted(ext))return this.container.classList.remove(this.selector),this.container.classList.add(this.reverseSelector),this.changeNameInput(""),void this.container.querySelector(".js-drop-zone-invalid-btn").addEventListener("click",function(){_this.container.classList.remove(_this.selector),_this.container.classList.remove(_this.reverseSelector),_this.container.querySelector(".js-drop-zone-file").value="",_this.changeNameInput("")});var id=event.target.dataset.id;if(Dropzone.setNameToItem(name,ext,this.data[id].item),this.changeNameInput(name),this.container.classList.remove(this.reverseSelector),this.container.classList.add(this.selector),this.count!==this.maxItems){this.count+=1,this.number+=1;var item=this.idealItem.cloneNode(!0);if(this.createRemover(item),this.fileContainer.insertBefore(this.data[id].item,this.fileContainer.firstChild),this.count!==this.maxItems){var input=this.idealInput.cloneNode(!0);input.setAttribute("data-id",this.number),input.addEventListener("change",this.addFile.bind(this)),this.data[this.number]={input:input,item:item},this.data[id].input.style.display="none",this.container.insertBefore(input,event.target)}}}},{key:"removeFile",value:function(event){var id=event.target.dataset.id;if(this.count===this.maxItems){var prevInput=this.container.querySelector(".js-drop-zone-file");prevInput.style.display="none";var _item=this.idealItem.cloneNode(!0);this.createRemover(_item);var _input=this.idealInput.cloneNode(!0);_input.setAttribute("data-id",this.number),_input.addEventListener("change",this.addFile.bind(this)),this.data[this.number]={input:_input,item:_item},this.container.insertBefore(_input,prevInput),this.changeNameInput("")}this.count-=1;var _data$id=this.data[id],item=_data$id.item,input=_data$id.input;item.parentNode.removeChild(item),input.parentNode.removeChild(input),this.container.querySelector(".js-drop-zone-file").style.display="block",0===this.count&&(this.container.classList.remove(this.selector),this.container.classList.remove(this.reverseSelector)),delete this.data[id]}},{key:"createRemover",value:function(item){var remover=item.querySelector(".js-drop-zone-btn");remover.setAttribute("data-id",this.number),remover.addEventListener("click",this.removeFile.bind(this))}},{key:"changeNameInput",value:function(value){this.nameInput&&(this.nameInput.hasAttribute("disabled")||(this.nameInput.value=value))}},{key:"isFormatAccepted",value:function(extension){return!this.acceptedFormatsStr||this.acceptedFormats.includes(extension)}}],[{key:"getFileFullName",value:function(file){var name=file.name,arrayName=name.split(".");return{name:name,ext:arrayName[arrayName.length-1]}}},{key:"setNameToItem",value:function(name,ext,item){item.querySelector(".js-drop-zone-name").innerHTML=name,item.querySelector(".js-drop-zone-ext").innerHTML="."+ext.toUpperCase()}}]),Dropzone}();exports.default=Dropzone}).call(this)}finally{}}});