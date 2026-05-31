import api from "./api";

export interface ChatMessage {
  role: "user" | "assistant";
  content: string;
}

export interface ChatRequest {
  messages: ChatMessage[];
  timeZone?: string;
}

export interface ChatResponse {
  reply: string;
}

function detectTimeZone(): string | undefined {
  try {
    return Intl.DateTimeFormat().resolvedOptions().timeZone || undefined;
  } catch {
    return undefined;
  }
}

const chatService = {
  send: (request: ChatRequest) =>
    api.post<ChatResponse>("/chat", {
      ...request,
      timeZone: request.timeZone ?? detectTimeZone(),
    }),
};

export default chatService;
