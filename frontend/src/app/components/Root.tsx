import { Outlet } from "react-router";

export default function Root() {
  return (
    <div className="min-h-screen bg-gradient-to-br from-cyan-50 via-blue-50 to-emerald-50">
      <Outlet />
    </div>
  );
}
