import NavButton from "../ui/NavButton";
import NemarisIcon from "../common/NemarisIcon";
export default function Header() {
  return (
    <div className="flex w-full items-center justify-between  bg-background p-4 px-8 text-foreground">
      <NemarisIcon />
      <NavButton text="Login" navTo="/login" />
    </div>
  );
}
