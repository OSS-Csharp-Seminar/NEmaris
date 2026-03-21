export type ToolInputSchema = {
  type: "object";
  properties: Record<string, unknown>;
  required?: string[];
};

export type ToolResponse = {
  content: Array<{
    type: "text";
    text: string;
  }>;
};

export type ToolRunner = (args: unknown) => Promise<ToolResponse>;

export type ToolDefinition = {
  name: string;
  description: string;
  inputSchema: ToolInputSchema;
  run: ToolRunner;
};
