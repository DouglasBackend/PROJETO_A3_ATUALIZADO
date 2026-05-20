import { useState, useEffect } from "react";
import { useNavigate } from "react-router";
import { fetchApi } from "../../lib/api";
import { Droplets, Mail, Lock, User, Home } from "lucide-react";
import { Button } from "./ui/button";
import { Input } from "./ui/input";
import { Label } from "./ui/label";
import { Card, CardContent, CardFooter, CardHeader } from "./ui/card";

export default function LoginPage() {
  const navigate = useNavigate();
  const [tela, setTela] = useState<"carregando" | "login" | "cadastro">("carregando");
  const [loginEmail, setLoginEmail] = useState("");
  const [loginPassword, setLoginPassword] = useState("");
  const [signupName, setSignupName] = useState("");
  const [signupAddress, setSignupAddress] = useState("");
  const [signupEmail, setSignupEmail] = useState("");
  const [signupPassword, setSignupPassword] = useState("");
  const [errorMsg, setErrorMsg] = useState("");

  useEffect(() => {
    fetchApi("/sistema/cadastrado")
      .then((data) => setTela(data.cadastrado ? "login" : "cadastro"))
      .catch(() => setTela("login"));
  }, []);

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMsg("");
    try {
      const data = await fetchApi("/autenticacao/entrar", {
        method: "POST",
        body: JSON.stringify({ email: loginEmail, senha: loginPassword }),
      });
      localStorage.setItem("userName", data.nome);
      navigate("/dashboard");
    } catch (error: any) {
      setErrorMsg("Falha no login: " + error.message);
    }
  };

  const handleSignup = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMsg("");
    try {
      await fetchApi("/autenticacao/registrar", {
        method: "POST",
        body: JSON.stringify({ nome: signupName, email: signupEmail, senha: signupPassword }),
      });
      alert("Cadastro realizado com sucesso! Faça login para continuar.");
      setLoginEmail(signupEmail);
      setSignupEmail("");
      setSignupName("");
      setSignupPassword("");
      setSignupAddress("");
      setTela("login");
    } catch (error: any) {
      setErrorMsg("Falha no cadastro: " + error.message);
    }
  };

  if (tela === "carregando") {
    return <div className="min-h-screen flex items-center justify-center">Carregando...</div>;
  }

  return (
    <div className="min-h-screen flex items-center justify-center p-4">
      <div className="w-full max-w-md space-y-6">
        <div className="text-center space-y-2">
          <div className="flex justify-center">
            <div className="bg-gradient-to-br from-cyan-500 to-emerald-500 p-4 rounded-2xl shadow-lg">
              <Droplets className="w-12 h-12 text-white" />
            </div>
          </div>
          <h1 className="text-3xl text-slate-800">AquaMonitor</h1>
          <p className="text-slate-600">Sistema de Monitoramento de Água</p>
        </div>

        <Card className="border-cyan-200 shadow-xl">
          {tela === "login" ? (
            <>
              <CardHeader>
                <h2 className="text-lg font-semibold text-slate-700 text-center">Entrar na conta</h2>
              </CardHeader>
              {errorMsg && <div className="p-3 bg-red-100 text-red-700 text-sm rounded mx-4">{errorMsg}</div>}
              <form onSubmit={handleLogin}>
                <CardContent className="space-y-4 pt-4">
                  <div className="space-y-2">
                    <Label htmlFor="login-email">E-mail</Label>
                    <div className="relative">
                      <Mail className="absolute left-3 top-3 w-4 h-4 text-slate-400" />
                      <Input
                        id="login-email"
                        type="email"
                        placeholder="seu@email.com"
                        className="pl-10 border-cyan-200 focus:border-cyan-500"
                        value={loginEmail}
                        onChange={(e) => setLoginEmail(e.target.value)}
                        required
                      />
                    </div>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="login-password">Senha</Label>
                    <div className="relative">
                      <Lock className="absolute left-3 top-3 w-4 h-4 text-slate-400" />
                      <Input
                        id="login-password"
                        type="password"
                        placeholder="••••••••"
                        className="pl-10 border-cyan-200 focus:border-cyan-500"
                        value={loginPassword}
                        onChange={(e) => setLoginPassword(e.target.value)}
                        required
                      />
                    </div>
                  </div>
                </CardContent>
                <CardFooter>
                  <Button
                    type="submit"
                    className="w-full bg-gradient-to-r from-cyan-600 to-cyan-500 hover:from-cyan-700 hover:to-cyan-600 text-white"
                  >
                    Entrar
                  </Button>
                </CardFooter>
              </form>
            </>
          ) : (
            <>
              <CardHeader>
                <h2 className="text-lg font-semibold text-slate-700 text-center">Criar conta</h2>
              </CardHeader>
              {errorMsg && <div className="p-3 bg-red-100 text-red-700 text-sm rounded mx-4">{errorMsg}</div>}
              <form onSubmit={handleSignup}>
                <CardContent className="space-y-4 pt-4">
                  <div className="space-y-2">
                    <Label htmlFor="signup-name">Nome Completo</Label>
                    <div className="relative">
                      <User className="absolute left-3 top-3 w-4 h-4 text-slate-400" />
                      <Input
                        id="signup-name"
                        type="text"
                        placeholder="Seu nome"
                        className="pl-10 border-emerald-200 focus:border-emerald-500"
                        value={signupName}
                        onChange={(e) => setSignupName(e.target.value)}
                        required
                      />
                    </div>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="signup-address">Endereço da Residência</Label>
                    <div className="relative">
                      <Home className="absolute left-3 top-3 w-4 h-4 text-slate-400" />
                      <Input
                        id="signup-address"
                        type="text"
                        placeholder="Rua, número, bairro"
                        className="pl-10 border-emerald-200 focus:border-emerald-500"
                        value={signupAddress}
                        onChange={(e) => setSignupAddress(e.target.value)}
                      />
                    </div>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="signup-email">E-mail</Label>
                    <div className="relative">
                      <Mail className="absolute left-3 top-3 w-4 h-4 text-slate-400" />
                      <Input
                        id="signup-email"
                        type="email"
                        placeholder="seu@email.com"
                        className="pl-10 border-emerald-200 focus:border-emerald-500"
                        value={signupEmail}
                        onChange={(e) => setSignupEmail(e.target.value)}
                        required
                      />
                    </div>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="signup-password">Senha</Label>
                    <div className="relative">
                      <Lock className="absolute left-3 top-3 w-4 h-4 text-slate-400" />
                      <Input
                        id="signup-password"
                        type="password"
                        placeholder="••••••••"
                        className="pl-10 border-emerald-200 focus:border-emerald-500"
                        value={signupPassword}
                        onChange={(e) => setSignupPassword(e.target.value)}
                        required
                      />
                    </div>
                  </div>
                </CardContent>
                <CardFooter>
                  <Button
                    type="submit"
                    className="w-full bg-gradient-to-r from-emerald-600 to-emerald-500 hover:from-emerald-700 hover:to-emerald-600 text-white"
                  >
                    Criar Conta
                  </Button>
                </CardFooter>
              </form>
            </>
          )}
        </Card>

        <p className="text-center text-sm text-slate-500">
          Monitore seu consumo de água e contribua para um futuro sustentável 💧
        </p>
      </div>
    </div>
  );
}
