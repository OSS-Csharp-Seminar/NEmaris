"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.allTools = void 0;
const nemarisCreateReservationTool_js_1 = require("./nemarisCreateReservationTool.js");
const nemarisGetAvailableTablesTool_js_1 = require("./nemarisGetAvailableTablesTool.js");
const nemarisListReservationsTool_js_1 = require("./nemarisListReservationsTool.js");
const nemarisLoginTool_js_1 = require("./nemarisLoginTool.js");
const ollamaGenerateTool_js_1 = require("./ollamaGenerateTool.js");
exports.allTools = [
    nemarisLoginTool_js_1.nemarisLoginTool,
    nemarisGetAvailableTablesTool_js_1.nemarisGetAvailableTablesTool,
    nemarisCreateReservationTool_js_1.nemarisCreateReservationTool,
    nemarisListReservationsTool_js_1.nemarisListReservationsTool,
    ollamaGenerateTool_js_1.ollamaGenerateTool,
];
