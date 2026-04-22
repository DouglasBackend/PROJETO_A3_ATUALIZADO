import { createBrowserRouter } from "react-router";
import Root from "./components/Root";
import LoginPage from "./components/LoginPage";
import Dashboard from "./components/Dashboard";
import DataEntry from "./components/DataEntry";
import EducationalTips from "./components/EducationalTips";

export const router = createBrowserRouter([
  {
    path: "/",
    Component: Root,
    children: [
      { index: true, Component: LoginPage },
      { path: "dashboard", Component: Dashboard },
      { path: "data-entry", Component: DataEntry },
      { path: "tips", Component: EducationalTips },
    ],
  },
]);
