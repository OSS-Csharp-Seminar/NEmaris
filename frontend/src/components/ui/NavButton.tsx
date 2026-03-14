import { useNavigate } from "react-router-dom";

interface Props {
  text: string;
  navTo: string;
}

export default function NavButton({ text, navTo }: Props) {
  const navigate = useNavigate();

  return (
    <div
      onClick={() => navigate(navTo)}
      className="bg-primary text-primary-foreground w-fit rounded-lg px-4 py-2"
    >
      <p>{text}</p>
    </div>
  );
}
