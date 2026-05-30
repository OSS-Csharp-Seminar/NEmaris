import { useState } from "react";
import { Outlet } from "react-router-dom";
import Header from "./components/layout/Header";
import ChatWidget from "./components/ui/ChatWidget";

function App() {
  const [isChatOpen, setIsChatOpen] = useState(false);

  return (
    <div className="h-screen flex flex-col overflow-hidden">
      <Header onOpenChat={() => setIsChatOpen(true)} />
      <main className="flex-1 min-h-0 overflow-hidden p-4">
        <Outlet />
      </main>
      <ChatWidget open={isChatOpen} onOpenChange={setIsChatOpen} />
    </div>
  );
}

export default App;
