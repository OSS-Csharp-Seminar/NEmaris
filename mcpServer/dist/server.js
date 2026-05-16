"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const index_js_1 = require("@modelcontextprotocol/sdk/server/index.js");
const stdio_js_1 = require("@modelcontextprotocol/sdk/server/stdio.js");
const types_js_1 = require("@modelcontextprotocol/sdk/types.js");
const index_js_2 = require("./src/tools/index.js");
const server = new index_js_1.Server({
    name: "nemaris-mcp-server",
    version: "1.1.0",
}, {
    capabilities: {
        tools: {},
    },
});
server.setRequestHandler(types_js_1.ListToolsRequestSchema, async () => {
    return {
        tools: index_js_2.allTools.map((tool) => ({
            name: tool.name,
            description: tool.description,
            inputSchema: tool.inputSchema,
        })),
    };
});
server.setRequestHandler(types_js_1.CallToolRequestSchema, async (req) => {
    const tool = index_js_2.allTools.find((item) => item.name === req.params.name);
    if (!tool)
        throw new Error(`Unknown tool: ${req.params.name}`);
    return tool.run(req.params.arguments ?? {});
});
async function main() {
    console.error("MCP server started");
    const transport = new stdio_js_1.StdioServerTransport();
    await server.connect(transport);
}
main().catch((err) => {
    console.error("Fatal error:", err);
    process.exit(1);
});
