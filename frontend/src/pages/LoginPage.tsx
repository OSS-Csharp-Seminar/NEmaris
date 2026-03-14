import LoginForm from "../components/ui/LoginForm";

export default function LoginPage() {
  return (
    <div className="min-h-screen bg-gray-50 flex items-start justify-center">
      <div className="flex w-full max-w-6xl flex-row gap-6 rounded-3xl border border-gray-200 bg-white p-6 shadow-lg">

        <div className="w-[420px]">
          <LoginForm />
        </div>

        <div className="flex-1 rounded-3xl bg-slate-50 p-6">
          <h1 className="text-3xl font-bold text-gray-800 mb-2">
            Overview
          </h1>

          <p className="text-sm text-gray-500 mb-6">
            Manage reservations, tables and guests from one place.
          </p>

          <div className="grid grid-cols-2 gap-4">

            <div className="rounded-2xl border border-gray-200 bg-white p-5 shadow-sm">
              <p className="text-sm text-gray-500">Total tables</p>
              <h2 className="mt-2 text-3xl font-bold text-gray-800">--</h2>
            </div>

            <div className="rounded-2xl border border-gray-200 bg-white p-5 shadow-sm">
              <p className="text-sm text-gray-500">Occupied tables</p>
              <h2 className="mt-2 text-3xl font-bold text-red-500">--</h2>
            </div>

            <div className="rounded-2xl border border-gray-200 bg-white p-5 shadow-sm">
              <p className="text-sm text-gray-500">Reservations today</p>
              <h2 className="mt-2 text-3xl font-bold text-blue-500">--</h2>
            </div>

            <div className="rounded-2xl border border-gray-200 bg-white p-5 shadow-sm">
              <p className="text-sm text-gray-500">Pending reservations</p>
              <h2 className="mt-2 text-3xl font-bold text-yellow-500">--</h2>
            </div>

          </div>

          <div className="mt-6 rounded-2xl border border-gray-200 bg-white p-5 shadow-sm">
            <h3 className="text-lg font-semibold text-gray-800 mb-3">
              Nesto nesto nesto 
            </h3>
          </div>

        </div>
      </div>
    </div>
  );
}
