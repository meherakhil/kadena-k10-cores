webpackJsonp([7],{

/***/ 260:
/***/ (function(module, exports, __webpack_require__) {

eval("/* WEBPACK VAR INJECTION */(function(module) {/* REACT HOT LOADER */ if (true) { (function () { var ReactHotAPI = __webpack_require__(48), RootInstanceProvider = __webpack_require__(49), ReactMount = __webpack_require__(36), React = __webpack_require__(7); module.makeHot = module.hot.data ? module.hot.data.makeHot : ReactHotAPI(function () { return RootInstanceProvider.getRootInstances(ReactMount); }, React); })(); } try { (function () {\n\n'use strict';\n\nObject.defineProperty(exports, \"__esModule\", {\n  value: true\n});\n\nvar _createClass = function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if (\"value\" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; }();\n\nfunction _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError(\"Cannot call a class as a function\"); } }\n\nvar Sidebar = function () {\n  function Sidebar(container) {\n    _classCallCheck(this, Sidebar);\n\n    this.container = container;\n    this.startSidebarPos = -1;\n    this.animatedClass = 'isFixed';\n    this.initScroll();\n  }\n\n  _createClass(Sidebar, [{\n    key: 'initScroll',\n    value: function initScroll() {\n      var _this = this;\n\n      window.addEventListener('scroll', function () {\n        var bar = _this.container;\n        if (_this.startSidebarPos < 0) {\n          _this.startSidebarPos = Sidebar.findPosY(bar);\n        }\n\n        if (window.pageYOffset > _this.startSidebarPos) {\n          bar.classList.add(_this.animatedClass);\n        } else {\n          bar.classList.remove(_this.animatedClass);\n        }\n      });\n    }\n  }], [{\n    key: 'findPosY',\n    value: function findPosY(obj) {\n      var curtop = 0;\n      if (typeof obj.offsetParent !== 'undefined' && obj.offsetParent) {\n        while (obj.offsetParent) {\n          curtop += obj.offsetTop;\n          obj = obj.offsetParent;\n        }\n        curtop += obj.offsetTop;\n      } else if (obj.y) {\n        curtop += obj.y;\n      }\n      return curtop;\n    }\n  }]);\n\n  return Sidebar;\n}();\n\nexports.default = Sidebar;\n\n/* REACT HOT LOADER */ }).call(this); } finally { if (true) { (function () { var foundReactClasses = module.hot.data && module.hot.data.foundReactClasses || false; if (module.exports && module.makeHot) { var makeExportsHot = __webpack_require__(50); if (makeExportsHot(module, __webpack_require__(7))) { foundReactClasses = true; } var shouldAcceptModule = true && foundReactClasses; if (shouldAcceptModule) { module.hot.accept(function (err) { if (err) { console.error(\"Cannot apply hot update to \" + \"index.js\" + \": \" + err.message); } }); } } module.hot.dispose(function (data) { data.makeHot = module.makeHot; data.foundReactClasses = foundReactClasses; }); })(); } }\n/* WEBPACK VAR INJECTION */}.call(exports, __webpack_require__(43)(module)))//# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiMjYwLmpzIiwic291cmNlcyI6WyJ3ZWJwYWNrOi8vLy4vc3JjL2FwcC9zdGF0aWMvc2lkZWJhci9pbmRleC5qcz8wOTNjIl0sInNvdXJjZXNDb250ZW50IjpbIi8qIFJFQUNUIEhPVCBMT0FERVIgKi8gaWYgKG1vZHVsZS5ob3QpIHsgKGZ1bmN0aW9uICgpIHsgdmFyIFJlYWN0SG90QVBJID0gcmVxdWlyZShcImM6XFxcXGluZXRwdWJcXFxcd3d3cm9vdFxcXFxLYWRlbmEtazEwLWNvcmVcXFxcZnJvbnRlbmRcXFxcbm9kZV9tb2R1bGVzXFxcXHJlYWN0LWhvdC1hcGlcXFxcbW9kdWxlc1xcXFxpbmRleC5qc1wiKSwgUm9vdEluc3RhbmNlUHJvdmlkZXIgPSByZXF1aXJlKFwiYzpcXFxcaW5ldHB1YlxcXFx3d3dyb290XFxcXEthZGVuYS1rMTAtY29yZVxcXFxmcm9udGVuZFxcXFxub2RlX21vZHVsZXNcXFxccmVhY3QtaG90LWxvYWRlclxcXFxSb290SW5zdGFuY2VQcm92aWRlci5qc1wiKSwgUmVhY3RNb3VudCA9IHJlcXVpcmUoXCJyZWFjdC1kb20vbGliL1JlYWN0TW91bnRcIiksIFJlYWN0ID0gcmVxdWlyZShcInJlYWN0XCIpOyBtb2R1bGUubWFrZUhvdCA9IG1vZHVsZS5ob3QuZGF0YSA/IG1vZHVsZS5ob3QuZGF0YS5tYWtlSG90IDogUmVhY3RIb3RBUEkoZnVuY3Rpb24gKCkgeyByZXR1cm4gUm9vdEluc3RhbmNlUHJvdmlkZXIuZ2V0Um9vdEluc3RhbmNlcyhSZWFjdE1vdW50KTsgfSwgUmVhY3QpOyB9KSgpOyB9IHRyeSB7IChmdW5jdGlvbiAoKSB7XG5cbid1c2Ugc3RyaWN0JztcblxuT2JqZWN0LmRlZmluZVByb3BlcnR5KGV4cG9ydHMsIFwiX19lc01vZHVsZVwiLCB7XG4gIHZhbHVlOiB0cnVlXG59KTtcblxudmFyIF9jcmVhdGVDbGFzcyA9IGZ1bmN0aW9uICgpIHsgZnVuY3Rpb24gZGVmaW5lUHJvcGVydGllcyh0YXJnZXQsIHByb3BzKSB7IGZvciAodmFyIGkgPSAwOyBpIDwgcHJvcHMubGVuZ3RoOyBpKyspIHsgdmFyIGRlc2NyaXB0b3IgPSBwcm9wc1tpXTsgZGVzY3JpcHRvci5lbnVtZXJhYmxlID0gZGVzY3JpcHRvci5lbnVtZXJhYmxlIHx8IGZhbHNlOyBkZXNjcmlwdG9yLmNvbmZpZ3VyYWJsZSA9IHRydWU7IGlmIChcInZhbHVlXCIgaW4gZGVzY3JpcHRvcikgZGVzY3JpcHRvci53cml0YWJsZSA9IHRydWU7IE9iamVjdC5kZWZpbmVQcm9wZXJ0eSh0YXJnZXQsIGRlc2NyaXB0b3Iua2V5LCBkZXNjcmlwdG9yKTsgfSB9IHJldHVybiBmdW5jdGlvbiAoQ29uc3RydWN0b3IsIHByb3RvUHJvcHMsIHN0YXRpY1Byb3BzKSB7IGlmIChwcm90b1Byb3BzKSBkZWZpbmVQcm9wZXJ0aWVzKENvbnN0cnVjdG9yLnByb3RvdHlwZSwgcHJvdG9Qcm9wcyk7IGlmIChzdGF0aWNQcm9wcykgZGVmaW5lUHJvcGVydGllcyhDb25zdHJ1Y3Rvciwgc3RhdGljUHJvcHMpOyByZXR1cm4gQ29uc3RydWN0b3I7IH07IH0oKTtcblxuZnVuY3Rpb24gX2NsYXNzQ2FsbENoZWNrKGluc3RhbmNlLCBDb25zdHJ1Y3RvcikgeyBpZiAoIShpbnN0YW5jZSBpbnN0YW5jZW9mIENvbnN0cnVjdG9yKSkgeyB0aHJvdyBuZXcgVHlwZUVycm9yKFwiQ2Fubm90IGNhbGwgYSBjbGFzcyBhcyBhIGZ1bmN0aW9uXCIpOyB9IH1cblxudmFyIFNpZGViYXIgPSBmdW5jdGlvbiAoKSB7XG4gIGZ1bmN0aW9uIFNpZGViYXIoY29udGFpbmVyKSB7XG4gICAgX2NsYXNzQ2FsbENoZWNrKHRoaXMsIFNpZGViYXIpO1xuXG4gICAgdGhpcy5jb250YWluZXIgPSBjb250YWluZXI7XG4gICAgdGhpcy5zdGFydFNpZGViYXJQb3MgPSAtMTtcbiAgICB0aGlzLmFuaW1hdGVkQ2xhc3MgPSAnaXNGaXhlZCc7XG4gICAgdGhpcy5pbml0U2Nyb2xsKCk7XG4gIH1cblxuICBfY3JlYXRlQ2xhc3MoU2lkZWJhciwgW3tcbiAgICBrZXk6ICdpbml0U2Nyb2xsJyxcbiAgICB2YWx1ZTogZnVuY3Rpb24gaW5pdFNjcm9sbCgpIHtcbiAgICAgIHZhciBfdGhpcyA9IHRoaXM7XG5cbiAgICAgIHdpbmRvdy5hZGRFdmVudExpc3RlbmVyKCdzY3JvbGwnLCBmdW5jdGlvbiAoKSB7XG4gICAgICAgIHZhciBiYXIgPSBfdGhpcy5jb250YWluZXI7XG4gICAgICAgIGlmIChfdGhpcy5zdGFydFNpZGViYXJQb3MgPCAwKSB7XG4gICAgICAgICAgX3RoaXMuc3RhcnRTaWRlYmFyUG9zID0gU2lkZWJhci5maW5kUG9zWShiYXIpO1xuICAgICAgICB9XG5cbiAgICAgICAgaWYgKHdpbmRvdy5wYWdlWU9mZnNldCA+IF90aGlzLnN0YXJ0U2lkZWJhclBvcykge1xuICAgICAgICAgIGJhci5jbGFzc0xpc3QuYWRkKF90aGlzLmFuaW1hdGVkQ2xhc3MpO1xuICAgICAgICB9IGVsc2Uge1xuICAgICAgICAgIGJhci5jbGFzc0xpc3QucmVtb3ZlKF90aGlzLmFuaW1hdGVkQ2xhc3MpO1xuICAgICAgICB9XG4gICAgICB9KTtcbiAgICB9XG4gIH1dLCBbe1xuICAgIGtleTogJ2ZpbmRQb3NZJyxcbiAgICB2YWx1ZTogZnVuY3Rpb24gZmluZFBvc1kob2JqKSB7XG4gICAgICB2YXIgY3VydG9wID0gMDtcbiAgICAgIGlmICh0eXBlb2Ygb2JqLm9mZnNldFBhcmVudCAhPT0gJ3VuZGVmaW5lZCcgJiYgb2JqLm9mZnNldFBhcmVudCkge1xuICAgICAgICB3aGlsZSAob2JqLm9mZnNldFBhcmVudCkge1xuICAgICAgICAgIGN1cnRvcCArPSBvYmoub2Zmc2V0VG9wO1xuICAgICAgICAgIG9iaiA9IG9iai5vZmZzZXRQYXJlbnQ7XG4gICAgICAgIH1cbiAgICAgICAgY3VydG9wICs9IG9iai5vZmZzZXRUb3A7XG4gICAgICB9IGVsc2UgaWYgKG9iai55KSB7XG4gICAgICAgIGN1cnRvcCArPSBvYmoueTtcbiAgICAgIH1cbiAgICAgIHJldHVybiBjdXJ0b3A7XG4gICAgfVxuICB9XSk7XG5cbiAgcmV0dXJuIFNpZGViYXI7XG59KCk7XG5cbmV4cG9ydHMuZGVmYXVsdCA9IFNpZGViYXI7XG5cbi8qIFJFQUNUIEhPVCBMT0FERVIgKi8gfSkuY2FsbCh0aGlzKTsgfSBmaW5hbGx5IHsgaWYgKG1vZHVsZS5ob3QpIHsgKGZ1bmN0aW9uICgpIHsgdmFyIGZvdW5kUmVhY3RDbGFzc2VzID0gbW9kdWxlLmhvdC5kYXRhICYmIG1vZHVsZS5ob3QuZGF0YS5mb3VuZFJlYWN0Q2xhc3NlcyB8fCBmYWxzZTsgaWYgKG1vZHVsZS5leHBvcnRzICYmIG1vZHVsZS5tYWtlSG90KSB7IHZhciBtYWtlRXhwb3J0c0hvdCA9IHJlcXVpcmUoXCJjOlxcXFxpbmV0cHViXFxcXHd3d3Jvb3RcXFxcS2FkZW5hLWsxMC1jb3JlXFxcXGZyb250ZW5kXFxcXG5vZGVfbW9kdWxlc1xcXFxyZWFjdC1ob3QtbG9hZGVyXFxcXG1ha2VFeHBvcnRzSG90LmpzXCIpOyBpZiAobWFrZUV4cG9ydHNIb3QobW9kdWxlLCByZXF1aXJlKFwicmVhY3RcIikpKSB7IGZvdW5kUmVhY3RDbGFzc2VzID0gdHJ1ZTsgfSB2YXIgc2hvdWxkQWNjZXB0TW9kdWxlID0gdHJ1ZSAmJiBmb3VuZFJlYWN0Q2xhc3NlczsgaWYgKHNob3VsZEFjY2VwdE1vZHVsZSkgeyBtb2R1bGUuaG90LmFjY2VwdChmdW5jdGlvbiAoZXJyKSB7IGlmIChlcnIpIHsgY29uc29sZS5lcnJvcihcIkNhbm5vdCBhcHBseSBob3QgdXBkYXRlIHRvIFwiICsgXCJpbmRleC5qc1wiICsgXCI6IFwiICsgZXJyLm1lc3NhZ2UpOyB9IH0pOyB9IH0gbW9kdWxlLmhvdC5kaXNwb3NlKGZ1bmN0aW9uIChkYXRhKSB7IGRhdGEubWFrZUhvdCA9IG1vZHVsZS5tYWtlSG90OyBkYXRhLmZvdW5kUmVhY3RDbGFzc2VzID0gZm91bmRSZWFjdENsYXNzZXM7IH0pOyB9KSgpOyB9IH1cblxuXG4vLy8vLy8vLy8vLy8vLy8vLy9cbi8vIFdFQlBBQ0sgRk9PVEVSXG4vLyAuL3NyYy9hcHAvc3RhdGljL3NpZGViYXIvaW5kZXguanNcbi8vIG1vZHVsZSBpZCA9IDI2MFxuLy8gbW9kdWxlIGNodW5rcyA9IDciXSwibWFwcGluZ3MiOiJBQUFBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBIiwic291cmNlUm9vdCI6IiJ9");

/***/ })

});