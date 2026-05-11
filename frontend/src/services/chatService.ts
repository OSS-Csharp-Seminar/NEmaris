import api from "./api";

export interface ChatMessage {
  role: "user" | "assistant";
  content: string;
}

export interface ChatRequest {
  messages: ChatMessage[];
}

export interface ChatResponse {
  reply: string;
}

const chatService = {
  send: (request: ChatRequest) => api.post<ChatResponse>("/chat", request),
};

export default chatService;
