import { useState } from "react";
import { Outlet } from "react-router-dom";
import Header from "./components/layout/Header";
import ChatWidget, { type ChatVariant } from "./components/ui/ChatWidget";

export interface AppOutletContext {
  openChat: (variant?: ChatVariant, initialMessage?: string) => void;
}

function App() {
  const [isChatOpen, setIsChatOpen] = useState(false);
  const [chatVariant, setChatVariant] = useState<ChatVariant>("compact");
  const [pendingMessage, setPendingMessage] = useState<string | null>(null);

  const openChat = (variant: ChatVariant = "compact", initialMessage?: string) => {
    setChatVariant(variant);
    if (initialMessage && initialMessage.trim()) {
      setPendingMessage(initialMessage.trim());
    }
    setIsChatOpen(true);
  };

  return (
    <div className="h-screen flex flex-col overflow-hidden">
      <Header onOpenChat={() => openChat("compact")} />
      <main className="flex-1 min-h-0 overflow-hidden p-4">
        <Outlet context={{ openChat } satisfies AppOutletContext} />
      </main>
      <ChatWidget
        open={isChatOpen}
        onOpenChange={setIsChatOpen}
        variant={chatVariant}
        pendingMessage={pendingMessage}
        onPendingMessageConsumed={() => setPendingMessage(null)}
      />
    </div>
  );
}

export default App;
