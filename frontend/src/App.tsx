import { Outlet } from "react-router-dom";
import Header from "./components/layout/Header";
import ChatWidget from "./components/ui/ChatWidget";

function App() {
  return (
    <div className="h-screen flex flex-col overflow-hidden">
      <Header />
      <main className="flex-1 min-h-0 overflow-hidden p-4">
        <Outlet />
      </main>
      <ChatWidget />
    </div>
  );
}

export default App;
