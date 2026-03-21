import { useNavigate } from "react-router-dom";
import NemarisIcon from "../common/NemarisIcon";
import { useAuth } from "../../context/AuthContext";

export default function Header() {
  const { isAuthenticated, isAdmin, user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    navigate("/login", { replace: true });
  };

  return (
    <div className="flex w-full items-center justify-between bg-background p-4 px-8 text-foreground">
      <NemarisIcon />

      <div className="flex items-center gap-4">
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

            {isAdmin && (
              <div
                onClick={() => navigate("/register")}
                className="cursor-pointer rounded-lg border border-border px-4 py-2 text-sm transition-colors hover:bg-secondary"
              >
                Register User
              </div>
            )}

            <div
              onClick={handleLogout}
              className="cursor-pointer rounded-lg bg-primary px-4 py-2 text-primary-foreground text-sm"
            >
              Logout
            </div>
          </>
        ) : (
          <div
            onClick={() => navigate("/login")}
            className="cursor-pointer rounded-lg bg-primary px-4 py-2 text-primary-foreground text-sm"
          >
            Login
          </div>
        )}
      </div>
    </div>
  );
}
