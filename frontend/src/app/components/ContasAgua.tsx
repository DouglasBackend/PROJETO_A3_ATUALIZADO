import { useState, useEffect } from "react";
import { useNavigate } from "react-router";
import { fetchApi } from "../../lib/api";
import { ArrowLeft, Trash2, DollarSign, Droplets, TrendingUp, Plus } from "lucide-react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "./ui/card";
import { Button } from "./ui/button";
import { Input } from "./ui/input";
import { Label } from "./ui/label";
import { Badge } from "./ui/badge";

interface ContaAgua {
  id: number;
  mesReferencia: string;
  litrosConsumidos: number;
  valorPago: number;
}

interface Estimativa {
  precoMedioPorLitro: number;
  consumoMesAtual: number;
  estimativaMesAtual: number;
  projecaoProximoMes: number;
  historicoCustos: ContaAgua[];
}

export default function ContasAgua() {
  const navigate = useNavigate();
  const [contas, setContas] = useState<ContaAgua[]>([]);
  const [estimativa, setEstimativa] = useState<Estimativa | null>(null);
  const [mesReferencia, setMesReferencia] = useState("");
  const [litros, setLitros] = useState("");
  const [valor, setValor] = useState("");
  const [erro, setErro] = useState("");

  const carregarDados = async () => {
    try {
      const [contasData, estimativaData] = await Promise.all([
        fetchApi("/contas-agua"),
        fetchApi("/dashboard/estimativa"),
      ]);
      setContas(contasData);
      setEstimativa(estimativaData);
    } catch (err) {
      console.error(err);
    }
  };

  useEffect(() => {
    carregarDados();
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErro("");

    if (!mesReferencia || !litros || !valor) {
      setErro("Preencha todos os campos.");
      return;
    }

    try {
      await fetchApi("/contas-agua", {
        method: "POST",
        body: JSON.stringify({
          mesReferencia: `${mesReferencia}-01T00:00:00Z`,
          litrosConsumidos: parseFloat(litros),
          valorPago: parseFloat(valor),
        }),
      });
      setMesReferencia("");
      setLitros("");
      setValor("");
      carregarDados();
    } catch (err: any) {
      setErro("Erro ao salvar: " + err.message);
    }
  };

  const handleDeletar = async (id: number) => {
    try {
      await fetchApi(`/contas-agua/${id}`, { method: "DELETE" });
      carregarDados();
    } catch (err) {
      console.error(err);
    }
  };

  const formatarMes = (dataStr: string) => {
    const data = new Date(dataStr);
    return data.toLocaleDateString("pt-BR", { month: "long", year: "numeric" });
  };

  const temContas = contas.length > 0;

  return (
    <div className="min-h-screen bg-slate-50">
      <header className="bg-white border-b border-cyan-200 sticky top-0 z-10 shadow-sm">
        <div className="max-w-4xl mx-auto px-4 py-4 flex items-center gap-3">
          <Button variant="ghost" size="sm" onClick={() => navigate("/dashboard")}>
            <ArrowLeft className="w-4 h-4 mr-2" />
            Voltar
          </Button>
          <div>
            <h1 className="text-lg text-slate-800">Contas de Água</h1>
            <p className="text-xs text-slate-500">Histórico e estimativa de gastos</p>
          </div>
        </div>
      </header>

      <main className="max-w-4xl mx-auto px-4 py-6 space-y-6">

        {estimativa && temContas && (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <Card className="border-cyan-200 bg-gradient-to-br from-cyan-50 to-white shadow-sm">
              <CardHeader className="pb-2">
                <CardDescription>Preço médio por litro</CardDescription>
                <CardTitle className="text-2xl text-cyan-700">
                  R$ {estimativa.precoMedioPorLitro.toFixed(4)}
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="flex items-center gap-2 text-sm text-slate-600">
                  <Droplets className="w-4 h-4" />
                  <span>Com base nas contas salvas</span>
                </div>
              </CardContent>
            </Card>

            <Card className="border-emerald-200 bg-gradient-to-br from-emerald-50 to-white shadow-sm">
              <CardHeader className="pb-2">
                <CardDescription>Estimativa deste mês</CardDescription>
                <CardTitle className="text-2xl text-emerald-700">
                  R$ {estimativa.estimativaMesAtual.toFixed(2)}
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="flex items-center gap-2 text-sm text-slate-600">
                  <DollarSign className="w-4 h-4" />
                  <span>{estimativa.consumoMesAtual.toFixed(0)}L registrados</span>
                </div>
              </CardContent>
            </Card>

            <Card className="border-purple-200 bg-gradient-to-br from-purple-50 to-white shadow-sm">
              <CardHeader className="pb-2">
                <CardDescription>Projeção próximo mês</CardDescription>
                <CardTitle className="text-2xl text-purple-700">
                  R$ {estimativa.projecaoProximoMes.toFixed(2)}
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="flex items-center gap-2 text-sm text-slate-600">
                  <TrendingUp className="w-4 h-4" />
                  <span>Baseado no ritmo atual</span>
                </div>
              </CardContent>
            </Card>
          </div>
        )}

        {estimativa && !temContas && (
          <Card className="border-amber-200 bg-amber-50">
            <CardContent className="p-4 text-center text-amber-800 text-sm">
              Cadastre pelo menos uma conta para ver as estimativas de gasto.
            </CardContent>
          </Card>
        )}

        <Card className="border-cyan-200 shadow-sm">
          <CardHeader>
            <CardTitle className="text-slate-800 flex items-center gap-2">
              <Plus className="w-5 h-5" />
              Adicionar Conta
            </CardTitle>
            <CardDescription>Informe os dados da sua última fatura de água</CardDescription>
          </CardHeader>
          <CardContent>
            {erro && (
              <div className="mb-4 p-3 bg-red-100 text-red-700 text-sm rounded">{erro}</div>
            )}
            <form onSubmit={handleSubmit} className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div className="space-y-2">
                <Label htmlFor="mes">Mês de referência</Label>
                <Input
                  id="mes"
                  type="month"
                  value={mesReferencia}
                  onChange={(e) => setMesReferencia(e.target.value)}
                  className="border-cyan-200 focus:border-cyan-500"
                  required
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="litros">Litros consumidos</Label>
                <Input
                  id="litros"
                  type="number"
                  placeholder="Ex: 12000"
                  value={litros}
                  onChange={(e) => setLitros(e.target.value)}
                  className="border-cyan-200 focus:border-cyan-500"
                  min="0.1"
                  step="0.1"
                  required
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="valor">Valor pago (R$)</Label>
                <Input
                  id="valor"
                  type="number"
                  placeholder="Ex: 85.50"
                  value={valor}
                  onChange={(e) => setValor(e.target.value)}
                  className="border-cyan-200 focus:border-cyan-500"
                  min="0.01"
                  step="0.01"
                  required
                />
              </div>
              <div className="md:col-span-3">
                <Button
                  type="submit"
                  className="bg-gradient-to-r from-cyan-600 to-cyan-500 hover:from-cyan-700 hover:to-cyan-600 text-white"
                >
                  Salvar Conta
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>

        {temContas && (
          <Card className="border-slate-200 shadow-sm">
            <CardHeader>
              <CardTitle className="text-slate-800">Histórico de Contas</CardTitle>
              <CardDescription>Suas faturas cadastradas</CardDescription>
            </CardHeader>
            <CardContent className="p-0">
              <div className="divide-y divide-slate-100">
                {contas.map((conta) => (
                  <div key={conta.id} className="flex items-center justify-between px-6 py-4">
                    <div className="flex items-center gap-4">
                      <div className="bg-cyan-100 p-2 rounded-lg">
                        <Droplets className="w-4 h-4 text-cyan-600" />
                      </div>
                      <div>
                        <p className="text-sm font-medium text-slate-700 capitalize">
                          {formatarMes(conta.mesReferencia)}
                        </p>
                        <p className="text-xs text-slate-500">{conta.litrosConsumidos.toLocaleString("pt-BR")} litros</p>
                      </div>
                    </div>
                    <div className="flex items-center gap-4">
                      <Badge className="bg-emerald-100 text-emerald-800 text-sm">
                        R$ {conta.valorPago.toFixed(2)}
                      </Badge>
                      <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => handleDeletar(conta.id)}
                        className="text-slate-400 hover:text-red-500 hover:bg-red-50"
                      >
                        <Trash2 className="w-4 h-4" />
                      </Button>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        )}

        {!temContas && (
          <Card className="border-slate-200 shadow-sm">
            <CardContent className="p-8 text-center text-slate-500">
              Nenhuma conta cadastrada ainda. Adicione sua primeira fatura acima.
            </CardContent>
          </Card>
        )}
      </main>
    </div>
  );
}
