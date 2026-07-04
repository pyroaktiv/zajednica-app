import { useEffect, useState } from "react";
import { StyleSheet, Text, View } from "react-native";

export default function Index() {
  const [status, setStatus] = useState("checking...");

  useEffect(() => {
    fetch(`${process.env.EXPO_PUBLIC_API_URL}/health`)
      .then((r) => r.text())
      .then(setStatus)
      .catch(() => setStatus("no connection"));
  }, []);

  return (
    <View style={{ flex: 1, alignItems: "center", justifyContent: "center" }}>
      <Text>Backend: {status}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    alignItems: "center",
    justifyContent: "center",
  },
});
