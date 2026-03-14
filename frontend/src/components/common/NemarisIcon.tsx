import logo from "../../assets/nemaris_selected_logo.png";

export default function NemarisIcon() {
  return (
    <div className="flex flex-row items-center font-bold ">
      <img className="w-18 pb-2" src={logo} alt="NemarisLogo" />
      <p>nemaris</p>
    </div>
  );
}
