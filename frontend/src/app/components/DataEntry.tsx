import { useState, useEffect } from "react";
import { useNavigate } from "react-router";
import { fetchApi } from "../../lib/api";
import { Droplets, Calendar, Clock, Save, ArrowLeft, TrendingUp } from "lucide-react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "./ui/card";
import { Button } from "./ui/button";
import { Input } from "./ui/input";
import { Label } from "./ui/label";
import { Alert, AlertDescription } from "./ui/alert";

export default function DataEntry() {
  const navigate = useNavigate();
  const [reading, setReading] = useState("");
  const [date, setDate] = useState(new Date().toISOString().split('T')[0]);
  const [time, setTime] = useState(new Date().toTimeString().slice(0, 5));
  const [showSuccess, setShowSuccess] = useState(false);
  const [lastReading, setLastReading] = useState<number | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchLastReading = async () => {
      try {
        const data = await fetchApi("/registros-agua?pagina=1&tamanho=1");
        // Se houver uma última leitura, seta o valor. Se for null/undefined, permanece nulo.
        if (data && data.length > 0 && data[0].consumoLitros !== undefined) {
          setLastReading(data[0].consumoLitros);
        } else {
          setLastReading(null);
        }
      } catch (err) {
        console.error("Erro ao buscar última leitura:", err);
      } finally {
        setIsLoading(false);
      }
    };
    fetchLastReading();
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      await fetchApi("/registros-agua", {
        method: "POST",
        body: JSON.stringify({ 
          consumoLitros: parseInt(reading),
          data: `${date}T${time}:00Z`
        })
      });

      setShowSuccess(true);
      
      setTimeout(() => {
        navigate("/dashboard");
      }, 2000);
    } catch (err) {
      console.error("Erro ao salvar leitura:", err);
      alert("Falha ao salvar a leitura.");
    }
  };

  const calculatedConsumption = (reading && lastReading !== null) 
    ? parseInt(reading) - lastReading 
    : (reading && lastReading === null ? parseInt(reading) : 0); // Se não tem leitura anterior, o consumo é o próprio valor inserido ou zero. (Conforme o usuário pediu, ele pode cadastrar inicial)

  return (
    <div className="min-h-screen">
      {/* Header */}
      <header className="bg-white border-b border-cyan-200 sticky top-0 z-10 shadow-sm">
        <div className="max-w-4xl mx-auto px-4 py-4 flex items-center gap-3">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => navigate("/dashboard")}
            className="text-slate-600 hover:bg-slate-100"
          >
            <ArrowLeft className="w-4 h-4 mr-2" />
            Voltar
          </Button>
          <div className="flex items-center gap-3">
            <div className="bg-gradient-to-br from-cyan-500 to-emerald-500 p-2 rounded-lg">
              <Droplets className="w-6 h-6 text-white" />
            </div>
            <div>
              <h1 className="text-xl text-slate-800">Registrar Consumo</h1>
              <p className="text-xs text-slate-500">Adicione sua leitura do hidrômetro</p>
            </div>
          </div>
        </div>
      </header>

      <main className="max-w-4xl mx-auto px-4 py-8">
        {showSuccess && (
          <Alert className="mb-6 border-emerald-500 bg-emerald-50">
            <TrendingUp className="w-5 h-5 text-emerald-600" />
            <AlertDescription className="text-emerald-800 ml-2">
              <strong>Sucesso!</strong> Seus dados foram registrados. Redirecionando...
            </AlertDescription>
          </Alert>
        )}

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Formulário */}
          <Card className="lg:col-span-2 border-cyan-200">
            <CardHeader>
              <CardTitle className="text-slate-800">Nova Leitura</CardTitle>
              <CardDescription>Informe os dados da sua medição</CardDescription>
            </CardHeader>
            <CardContent>
              <form onSubmit={handleSubmit} className="space-y-6">
                <div className="space-y-2">
                  <Label htmlFor="reading">Leitura do Hidrômetro (Litros)</Label>
                  <div className="relative">
                    <Droplets className="absolute left-3 top-3 w-5 h-5 text-cyan-500" />
                    <Input
                      id="reading"
                      type="number"
                      placeholder="Digite a leitura atual"
                      className="pl-12 h-12 text-lg border-cyan-200 focus:border-cyan-500"
                      value={reading}
                      onChange={(e) => setReading(e.target.value)}
                      required
                      min={lastReading !== null ? lastReading : 0}
                    />
                  </div>
                  <p className="text-sm text-slate-500">
                    Última leitura: <span className="font-semibold">{lastReading !== null ? `${lastReading}L` : "Nenhuma (Primeiro Registro)"}</span>
                  </p>
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="date">Data</Label>
                    <div className="relative">
                      <Calendar className="absolute left-3 top-3 w-4 h-4 text-slate-400" />
                      <Input
                        id="date"
                        type="date"
                        className="pl-10 border-cyan-200 focus:border-cyan-500"
                        value={date}
                        onChange={(e) => setDate(e.target.value)}
                        required
                      />
                    </div>
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="time">Hora</Label>
                    <div className="relative">
                      <Clock className="absolute left-3 top-3 w-4 h-4 text-slate-400" />
                      <Input
                        id="time"
                        type="time"
                        className="pl-10 border-cyan-200 focus:border-cyan-500"
                        value={time}
                        onChange={(e) => setTime(e.target.value)}
                        required
                      />
                    </div>
                  </div>
                </div>

                {calculatedConsumption > 0 && (
                  <div className="p-4 bg-cyan-50 rounded-lg border border-cyan-200">
                    <p className="text-sm text-slate-600 mb-1">Consumo calculado:</p>
                    <p className="text-3xl font-semibold text-cyan-700">
                      {calculatedConsumption} <span className="text-lg">litros</span>
                    </p>
                    <p className="text-xs text-slate-500 mt-1">
                      Desde a última leitura
                    </p>
                  </div>
                )}

                <Button 
                  type="submit" 
                  className="w-full h-12 bg-gradient-to-r from-cyan-600 to-cyan-500 hover:from-cyan-700 hover:to-cyan-600 text-white"
                  disabled={!reading || showSuccess}
                >
                  <Save className="w-5 h-5 mr-2" />
                  Salvar Leitura
                </Button>
              </form>
            </CardContent>
          </Card>

          {/* Informações e Histórico */}
          <div className="space-y-6">
            <Card className="border-blue-200 bg-gradient-to-br from-blue-50 to-white">
              <CardHeader>
                <CardTitle className="text-blue-800 text-lg">Como ler o hidrômetro</CardTitle>
              </CardHeader>
              <CardContent className="space-y-3 text-sm text-slate-700">
                <div className="flex gap-3">
                  <div className="bg-blue-100 rounded-full w-6 h-6 flex items-center justify-center flex-shrink-0 mt-0.5">
                    <span className="text-blue-700 font-semibold text-xs">1</span>
                  </div>
                  <p>Localize seu hidrômetro (geralmente próximo ao portão)</p>
                </div>
                <div className="flex gap-3">
                  <div className="bg-blue-100 rounded-full w-6 h-6 flex items-center justify-center flex-shrink-0 mt-0.5">
                    <span className="text-blue-700 font-semibold text-xs">2</span>
                  </div>
                  <p>Anote os números pretos no visor (ignore os vermelhos)</p>
                </div>
                <div className="flex gap-3">
                  <div className="bg-blue-100 rounded-full w-6 h-6 flex items-center justify-center flex-shrink-0 mt-0.5">
                    <span className="text-blue-700 font-semibold text-xs">3</span>
                  </div>
                  <p>Digite o valor completo no campo acima</p>
                </div>
              </CardContent>
            </Card>

            <Card className="border-emerald-200 bg-gradient-to-br from-emerald-50 to-white">
              <CardHeader>
                <CardTitle className="text-emerald-800 text-lg">Últimas Leituras</CardTitle>
              </CardHeader>
              <CardContent className="space-y-3">
                <div className="flex justify-between items-center pb-2 border-b border-emerald-100">
                  <div>
                    <p className="text-sm font-semibold text-slate-700">Hoje, 09:30</p>
                    <p className="text-xs text-slate-500">234 litros</p>
                  </div>
                  <span className="text-lg font-bold text-emerald-600">-12L</span>
                </div>
                <div className="flex justify-between items-center pb-2 border-b border-emerald-100">
                  <div>
                    <p className="text-sm font-semibold text-slate-700">Ontem, 20:15</p>
                    <p className="text-xs text-slate-500">312 litros</p>
                  </div>
                  <span className="text-lg font-bold text-red-600">+89L</span>
                </div>
                <div className="flex justify-between items-center pb-2 border-b border-emerald-100">
                  <div>
                    <p className="text-sm font-semibold text-slate-700">23 Mar, 19:45</p>
                    <p className="text-xs text-slate-500">198 litros</p>
                  </div>
                  <span className="text-lg font-bold text-emerald-600">-24L</span>
                </div>
              </CardContent>
            </Card>

            <Card className="border-amber-200 bg-gradient-to-br from-amber-50 to-white">
              <CardContent className="p-4">
                <p className="text-sm text-amber-800">
                  💡 <strong>Dica:</strong> Registre suas leituras sempre no mesmo horário para obter dados mais precisos!
                </p>
              </CardContent>
            </Card>
          </div>
        </div>
      </main>
    </div>
  );
}
