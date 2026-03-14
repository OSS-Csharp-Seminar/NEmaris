import LoginForm from "../components/ui/LoginForm";

export default function LoginPage() {
  return (
    <div className=" flex flex-row bg-red-100 text-foreground h-full rounded-3xl border border-border p-4">
      <LoginForm />

      <h1>oden cemo stavit neke statove, npr broj zauteti stolova , i sl</h1>
    </div>
  );
}
