import { Redirect } from "expo-router";
import { useAuth } from "../state/AuthContext";
import { Loading } from "../ui/Basics";

export default function Index() {
  const { status } = useAuth();
  if (status === "loading") return <Loading />;
  return <Redirect href={status === "signedIn" ? "/home" : "/login"} />;
}
