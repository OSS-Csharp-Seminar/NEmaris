import { useNavigate } from "react-router-dom";
import NemarisIcon from "../common/NemarisIcon";
import { useAuth } from "../../context/useAuth";
import { useTheme } from "../../context/useTheme";

interface HeaderProps {
  onOpenChat: () => void;
}

export default function Header({ onOpenChat }: HeaderProps) {
  const { isAuthenticated, isAdmin, user, logout } = useAuth();
  const { theme, toggle } = useTheme();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    navigate("/login", { replace: true });
  };

  return (
    <div className="flex w-full items-center justify-between bg-background p-4 px-8 text-foreground">
      <NemarisIcon />

      <div className="flex items-center gap-4">
        <button
          type="button"
          onClick={toggle}
          aria-label={theme === "dark" ? "Switch to light mode" : "Switch to dark mode"}
          title={theme === "dark" ? "Switch to light mode" : "Switch to dark mode"}
          className="flex h-9 w-9 cursor-pointer items-center justify-center rounded-lg border border-border text-base transition-colors hover:bg-secondary"
        >
          {theme === "dark" ? "☀" : "☾"}
        </button>

        {isAuthenticated ? (
          <>
            <span className="text-sm text-muted-foreground">
              {user?.firstName} {user?.lastName}
              {isAdmin && (
                <span className="ml-2 rounded bg-blue-100 px-2 py-0.5 text-xs font-medium text-blue-700">
                  Admin
                </span>
              )}
            </span>

            <button
              type="button"
              onClick={() => navigate("/home")}
              className="cursor-pointer rounded-lg border border-border px-4 py-2 text-sm transition-colors hover:bg-secondary"
            >
              Stolovi
            </button>

            <button
              type="button"
              onClick={() => navigate("/orders")}
              className="cursor-pointer rounded-lg border border-border px-4 py-2 text-sm transition-colors hover:bg-secondary"
            >
              Narudžbe
            </button>

            <button
              type="button"
              onClick={() => navigate("/reservations")}
              className="cursor-pointer rounded-lg border border-border px-4 py-2 text-sm transition-colors hover:bg-secondary"
            >
              Rezervacije
            </button>

            <button
              type="button"
              onClick={onOpenChat}
              className="cursor-pointer rounded-lg border border-border px-4 py-2 text-sm transition-colors hover:bg-secondary"
            >
              Chat
            </button>

            {isAdmin && (
              <button
                type="button"
                onClick={() => navigate("/register")}
                className="cursor-pointer rounded-lg border border-border px-4 py-2 text-sm transition-colors hover:bg-secondary"
              >
                Register User
              </button>
            )}

            <button
              type="button"
              onClick={handleLogout}
              className="cursor-pointer rounded-lg bg-primary px-4 py-2 text-primary-foreground text-sm"
            >
              Logout
            </button>
          </>
        ) : (
          <button
            type="button"
            onClick={() => navigate("/login")}
            className="cursor-pointer rounded-lg bg-primary px-4 py-2 text-primary-foreground text-sm"
          >
            Login
          </button>
        )}
      </div>
    </div>
  );
}
