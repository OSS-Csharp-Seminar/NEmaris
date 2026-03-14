import NavButton from "./NavButton";
export default function LoginForm() {
  return (
    <div className="bg-blue-100 p-4 w-[50%] justify-center items-center  flex flex-col">
      <p> chatgptiraj neke gluposti da pisu iznad</p>
      <input
        className="border border-border bg-input my-4"
        placeholder="Username"
      />
      <input className="border border-border bg-input" placeholder="Password" />
      <NavButton text="di ide" navTo="ko ide" />
    </div>
  );
}
