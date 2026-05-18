import { Link } from "react-router-dom";

export default function HomePage() {
  return (
    <div className="min-h-screen bg-yellow-200 flex flex-col items-center justify-center gap-6">
      <h1 className="text-3xl font-bold text-gray-800">
        Home Page
      </h1>

      <Link
        to="/menu"
        className="rounded-xl bg-black px-6 py-3 text-white font-medium transition-opacity hover:opacity-80"
      >
        Open Menu Management
      </Link>
    </div>
  );
}