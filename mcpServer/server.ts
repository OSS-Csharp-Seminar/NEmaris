import { Server } from "@modelcontextprotocol/sdk/server/index.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import {
  ListToolsRequestSchema,
  CallToolRequestSchema,
} from "@modelcontextprotocol/sdk/types.js";

const server = new Server(
  {
    name: "ollama-server",
    version: "1.0.0",
  },
  {
    capabilities: {
      tools: {},
    },
  }
);

server.setRequestHandler(ListToolsRequestSchema, async () => {
  return {
    tools: [
      {
        name: "generate",
        description: "Generate text using Ollama",
        inputSchema: {
          type: "object",
          properties: {
            model: { type: "string" },
            prompt: { type: "string" },
          },
          required: ["model", "prompt"],
        },
      },
    ],
  };
});

server.setRequestHandler(CallToolRequestSchema, async (req) => {
  if (req.params.name === "generate") {
    const { model, prompt } = req.params.arguments as {
      model: string;
      prompt: string;
    };

    const res = await fetch("http://localhost:11434/api/generate", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        model,
        prompt,
        stream: false,
      }),
    });

    if (!res.ok) {
      const text = await res.text();
      throw new Error(`Ollama error ${res.status}: ${text}`);
    }

    const data = (await res.json()) as { response?: string };

    return {
      content: [
        {
          type: "text",
          text: data.response ?? "No response from Ollama",
        },
      ],
    };
  }

  throw new Error("Unknown tool");
});

async function main() {
  console.error("MCP server started");

  //maknit kasnije, ovo je samo da vidimo jel radi server
  setInterval(() => {
    const now = new Date().toLocaleTimeString();
    console.error("Time:", now);
  }, 5000);

  const transport = new StdioServerTransport();
  await server.connect(transport);
}

main().catch((err) => {
  console.error("Fatal error:", err);
  process.exit(1);
});
