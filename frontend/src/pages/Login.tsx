import React, { useState } from "react";
import { api } from "../services/api";
import { useNavigate, Link } from "react-router";

export default function Login() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [err, setErr] = useState<string | null>(null);
  const navigate = useNavigate();

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    try {
      setErr(null);
      const res = await api.login(email, password);
      localStorage.setItem("clubs_user", JSON.stringify(res));
      navigate("/profile");
    } catch (e: any) {
      setErr(e.message || "Giriş başarısız");
    }
  }

  return (
    <div className="max-w-md mx-auto bg-white p-6 rounded-md shadow">
      <h2 className="text-xl font-bold mb-4">Giriş Yap</h2>
      {err && <div className="text-red-500">{err}</div>}
      <form onSubmit={handleSubmit} className="space-y-3">
        <input value={email} onChange={e => setEmail(e.target.value)} placeholder="Email" type="email" className="w-full p-2 border rounded" required />
        <input value={password} onChange={e => setPassword(e.target.value)} placeholder="Şifre" type="password" className="w-full p-2 border rounded" required />
        <button className="w-full py-2 bg-accent rounded border">Giriş</button>
      </form>
      <p className="text-sm mt-3">Hesabın yok mu? <Link to="/register" className="text-accent">Kayıt ol</Link></p>
    </div>
  );
}
