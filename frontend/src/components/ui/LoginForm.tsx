import NavButton from "./NavButton";

export default function LoginForm() {
  return (
    <div className="w-full max-w-md rounded-2xl bg-white p-8 shadow-lg flex flex-col items-center pt-24">
      <h2 className="mb-2 text-2xl font-bold text-gray-800">Welcome</h2>
      <p className="mb-6 text-sm text-gray-500">
        Sign in to continue to your account
      </p>

      <input
        className="mb-4 w-full rounded-lg border border-gray-300 px-4 py-3 outline-none focus:border-blue-500"
        placeholder="Username"
      />

      <input
        className="mb-6 w-full rounded-lg border border-gray-300 px-4 py-3 outline-none focus:border-blue-500"
        placeholder="Password"
        type="password"
      />

      <NavButton text="Login" navTo="/home" />
    </div>
  );
}
