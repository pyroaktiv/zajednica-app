import { useState } from "react";
import { ScrollView } from "react-native";
import type { CommunityDataRequest } from "../api/types";
import { Button, ErrorText, Field } from "./Basics";
import { spacing } from "./theme";

export function CommunityForm({
  initial,
  submitTitle,
  onSubmit,
}: {
  initial?: CommunityDataRequest;
  submitTitle: string;
  onSubmit: (request: CommunityDataRequest) => Promise<void>;
}) {
  const [name, setName] = useState(initial?.name ?? "");
  const [street, setStreet] = useState(initial?.address.street ?? "");
  const [number, setNumber] = useState(initial?.address.number ?? "");
  const [registrationNumber, setRegistrationNumber] = useState(initial?.registrationNumber ?? "");
  const [taxId, setTaxId] = useState(initial?.taxId ?? "");
  const [bankAccountNumber, setBankAccountNumber] = useState(initial?.bankAccountNumber ?? "");
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const submit = async () => {
    setError(null);
    setBusy(true);
    try {
      await onSubmit({
        name: name.trim(),
        address: {
          street: street.trim(),
          number: number.trim(),
          latitude: initial?.address.latitude ?? null,
          longitude: initial?.address.longitude ?? null,
        },
        registrationNumber: registrationNumber.trim() || null,
        taxId: taxId.trim() || null,
        bankAccountNumber: bankAccountNumber.trim() || null,
      });
    } catch (e: any) {
      setError(e.message);
      setBusy(false);
    }
  };

  return (
    <ScrollView contentContainerStyle={{ padding: spacing.l }}>
      <Field label="Naziv zajednice *" value={name} onChangeText={setName} />
      <Field label="Ulica *" value={street} onChangeText={setStreet} />
      <Field label="Broj *" value={number} onChangeText={setNumber} />
      <Field label="Matični broj" value={registrationNumber} onChangeText={setRegistrationNumber} />
      <Field label="PIB" value={taxId} onChangeText={setTaxId} />
      <Field label="Tekući račun" value={bankAccountNumber} onChangeText={setBankAccountNumber} />
      <ErrorText error={error} />
      <Button title={submitTitle} onPress={submit} loading={busy} />
    </ScrollView>
  );
}
