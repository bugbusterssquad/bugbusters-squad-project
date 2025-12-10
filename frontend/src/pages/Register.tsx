import React, { useState } from "react";
import { api } from "../services/api";
import { useNavigate, Link } from "react-router";

export default function Register() {
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [msg, setMsg] = useState<string | null>(null);
  const navigate = useNavigate();

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    try {
      setMsg(null);
      const res = await api.register(name, email, password);
      setMsg(res.message || "Kayıt başarılı");
      navigate("/login");
    } catch (e: any) {
      setMsg(e.message || "Kayıt hatası");
    }
  }

  return (
    <div className="max-w-md mx-auto bg-white p-6 rounded-md shadow">
      <h2 className="text-xl font-bold mb-4">Kayıt Ol</h2>
      {msg && <div className="text-green-600">{msg}</div>}
      <form onSubmit={handleSubmit} className="space-y-3">
        <input value={name} onChange={e => setName(e.target.value)} placeholder="Ad Soyad" className="w-full p-2 border rounded" required />
        <input value={email} onChange={e => setEmail(e.target.value)} placeholder="Email" type="email" className="w-full p-2 border rounded" required />
        <input value={password} onChange={e => setPassword(e.target.value)} placeholder="Şifre" type="password" className="w-full p-2 border rounded" required />
        <button className="w-full py-2 bg-accent text-white rounded">Kayıt Ol</button>
      </form>
      <p className="text-sm mt-3">Zaten hesabın var mı? <Link to="/login" className="text-accent">Giriş yap</Link></p>
    </div>
  );
}
