import { useState } from "react";
import { useNavigate } from "react-router";
import { 
  Droplets, 
  ArrowLeft, 
  Lightbulb,
  Bath,
  Droplet,
  Leaf,
  Sprout,
  Clock,
  ChevronRight
} from "lucide-react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "./ui/card";
import { Button } from "./ui/button";
import { Badge } from "./ui/badge";
import { ImageWithFallback } from "./figma/ImageWithFallback";

const tips = [
  {
    id: 1,
    category: "Banho",
    icon: Bath,
    title: "Reduza o tempo do banho",
    description: "Diminuir o banho em 5 minutos pode economizar até 45 litros de água por dia.",
    savings: "45L/dia",
    impact: "high",
    image: "https://images.unsplash.com/photo-1610276173132-c47d148ab626?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHx3YXRlciUyMGZhdWNldCUyMGNsZWFufGVufDF8fHx8MTc3NDM5NTc2M3ww&ixlib=rb-4.1.0&q=80&w=1080&utm_source=figma&utm_medium=referral",
  },
  {
    id: 2,
    category: "Torneira",
    icon: Droplet,
    title: "Feche a torneira ao escovar os dentes",
    description: "Deixar a torneira aberta enquanto escova os dentes desperdiça até 12 litros.",
    savings: "12L/uso",
    impact: "medium",
    image: "https://images.unsplash.com/photo-1604994477975-399c19397739?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHx3YXRlciUyMGRyb3AlMjBjb25zZXJ2YXRpb258ZW58MXx8fHwxNzc0Mzk1NzYyfDA&ixlib=rb-4.1.0&q=80&w=1080&utm_source=figma&utm_medium=referral",
  },
  {
    id: 3,
    category: "Jardim",
    icon: Sprout,
    title: "Regue plantas pela manhã ou à noite",
    description: "Evite evaporação da água regando nos horários mais frescos do dia.",
    savings: "30L/dia",
    impact: "medium",
    image: "https://images.unsplash.com/photo-1764539572367-6095eb4f5c98?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxzdXN0YWluYWJsZSUyMGhvbWUlMjBuYXR1cmV8ZW58MXx8fHwxNzc0Mzk1NzYyfDA&ixlib=rb-4.1.0&q=80&w=1080&utm_source=figma&utm_medium=referral",
  },
  {
    id: 4,
    category: "Cozinha",
    icon: Droplets,
    title: "Lave louças com a torneira fechada",
    description: "Ensaboe toda a louça primeiro, depois enxágue tudo de uma vez.",
    savings: "20L/lavagem",
    impact: "medium",
    image: "https://images.unsplash.com/photo-1610276173132-c47d148ab626?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHx3YXRlciUyMGZhdWNldCUyMGNsZWFufGVufDF8fHx8MTc3NDM5NTc2M3ww&ixlib=rb-4.1.0&q=80&w=1080&utm_source=figma&utm_medium=referral",
  },
  {
    id: 5,
    category: "Lavanderia",
    icon: Clock,
    title: "Acumule roupas para lavar",
    description: "Use a máquina de lavar apenas com carga completa para otimizar o uso de água.",
    savings: "60L/lavagem",
    impact: "high",
    image: "https://images.unsplash.com/photo-1604994477975-399c19397739?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHx3YXRlciUyMGRyb3AlMjBjb25zZXJ2YXRpb258ZW58MXx8fHwxNzc0Mzk1NzYyfDA&ixlib=rb-4.1.0&q=80&w=1080&utm_source=figma&utm_medium=referral",
  },
  {
    id: 6,
    category: "Geral",
    icon: Leaf,
    title: "Conserte vazamentos imediatamente",
    description: "Uma torneira pingando pode desperdiçar até 46 litros por dia.",
    savings: "46L/dia",
    impact: "high",
    image: "https://images.unsplash.com/photo-1764539572367-6095eb4f5c98?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxzdXN0YWluYWJsZSUyMGhvbWUlMjBuYXR1cmV8ZW58MXx8fHwxNzc0Mzk1NzYyfDA&ixlib=rb-4.1.0&q=80&w=1080&utm_source=figma&utm_medium=referral",
  },
];

export default function EducationalTips() {
  const navigate = useNavigate();
  const [selectedCategory, setSelectedCategory] = useState<string>("all");

  const categories = ["all", "Banho", "Torneira", "Jardim", "Cozinha", "Lavanderia", "Geral"];
  const filteredTips = selectedCategory === "all" 
    ? tips 
    : tips.filter(tip => tip.category === selectedCategory);

  const getImpactColor = (impact: string) => {
    switch(impact) {
      case "high": return "bg-emerald-500";
      case "medium": return "bg-cyan-500";
      case "low": return "bg-blue-500";
      default: return "bg-slate-500";
    }
  };

  const getImpactLabel = (impact: string) => {
    switch(impact) {
      case "high": return "Alto Impacto";
      case "medium": return "Médio Impacto";
      case "low": return "Baixo Impacto";
      default: return "Impacto";
    }
  };

  return (
    <div className="min-h-screen">
      {/* Header */}
      <header className="bg-white border-b border-cyan-200 sticky top-0 z-10 shadow-sm">
        <div className="max-w-7xl mx-auto px-4 py-4 flex items-center gap-3">
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
            <div className="bg-gradient-to-br from-emerald-500 to-cyan-500 p-2 rounded-lg">
              <Lightbulb className="w-6 h-6 text-white" />
            </div>
            <div>
              <h1 className="text-xl text-slate-800">Dicas de Economia</h1>
              <p className="text-xs text-slate-500">Aprenda a economizar água no dia a dia</p>
            </div>
          </div>
        </div>
      </header>

      <main className="max-w-7xl mx-auto px-4 py-8 space-y-6">
        {/* Banner Educativo */}
        <Card className="border-emerald-300 bg-gradient-to-r from-emerald-500 to-cyan-500 text-white">
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div className="space-y-2">
                <h2 className="text-2xl">💧 Cada gota conta!</h2>
                <p className="text-emerald-50">
                  Implementando todas essas dicas, você pode economizar até <strong>200 litros por dia</strong>.
                </p>
                <p className="text-sm text-emerald-100">
                  Isso representa uma economia de cerca de <strong>R$ 60 por mês</strong> na sua conta!
                </p>
              </div>
              <Leaf className="w-24 h-24 opacity-20" />
            </div>
          </CardContent>
        </Card>

        {/* Filtros de Categoria */}
        <div className="flex flex-wrap gap-2">
          {categories.map((category) => (
            <Button
              key={category}
              variant={selectedCategory === category ? "default" : "outline"}
              onClick={() => setSelectedCategory(category)}
              className={
                selectedCategory === category 
                  ? "bg-cyan-600 hover:bg-cyan-700" 
                  : "border-cyan-300 text-cyan-700 hover:bg-cyan-50"
              }
            >
              {category === "all" ? "Todas" : category}
            </Button>
          ))}
        </div>

        {/* Grid de Dicas */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {filteredTips.map((tip) => {
            const IconComponent = tip.icon;
            return (
              <Card 
                key={tip.id} 
                className="border-cyan-200 hover:shadow-xl transition-all duration-300 hover:-translate-y-1 overflow-hidden"
              >
                <div className="relative h-48 bg-gradient-to-br from-cyan-100 to-blue-100 overflow-hidden">
                  <ImageWithFallback
                    src={tip.image}
                    alt={tip.title}
                    className="w-full h-full object-cover opacity-90"
                  />
                  <div className="absolute top-3 right-3">
                    <Badge className={`${getImpactColor(tip.impact)} text-white`}>
                      {getImpactLabel(tip.impact)}
                    </Badge>
                  </div>
                  <div className="absolute top-3 left-3 bg-white/90 backdrop-blur-sm rounded-full p-2">
                    <IconComponent className="w-5 h-5 text-cyan-600" />
                  </div>
                </div>
                <CardHeader>
                  <div className="flex items-start justify-between gap-2">
                    <div className="flex-1">
                      <Badge variant="outline" className="mb-2 text-xs border-cyan-300 text-cyan-700">
                        {tip.category}
                      </Badge>
                      <CardTitle className="text-lg text-slate-800">{tip.title}</CardTitle>
                    </div>
                  </div>
                  <CardDescription className="text-slate-600 leading-relaxed">
                    {tip.description}
                  </CardDescription>
                </CardHeader>
                <CardContent>
                  <div className="flex items-center justify-between p-3 bg-gradient-to-r from-emerald-50 to-cyan-50 rounded-lg border border-emerald-200">
                    <div className="flex items-center gap-2">
                      <Droplets className="w-5 h-5 text-emerald-600" />
                      <div>
                        <p className="text-xs text-slate-600">Economia</p>
                        <p className="text-lg font-bold text-emerald-700">{tip.savings}</p>
                      </div>
                    </div>
                    <ChevronRight className="w-5 h-5 text-slate-400" />
                  </div>
                </CardContent>
              </Card>
            );
          })}
        </div>

        {/* Card de Incentivo */}
        <Card className="border-blue-200 bg-gradient-to-br from-blue-50 to-white">
          <CardContent className="p-6">
            <div className="text-center space-y-3">
              <div className="inline-flex items-center justify-center bg-blue-100 rounded-full p-4">
                <Leaf className="w-8 h-8 text-blue-600" />
              </div>
              <h3 className="text-xl text-slate-800">Você está fazendo a diferença!</h3>
              <p className="text-slate-600 max-w-2xl mx-auto">
                Ao aplicar essas dicas, você não só economiza dinheiro, mas também contribui para a 
                preservação dos recursos hídricos do planeta. Cada litro economizado faz diferença!
              </p>
              <Button 
                onClick={() => navigate("/dashboard")}
                className="bg-gradient-to-r from-cyan-600 to-blue-600 hover:from-cyan-700 hover:to-blue-700 text-white mt-4"
              >
                Ver Meu Progresso
              </Button>
            </div>
          </CardContent>
        </Card>
      </main>
    </div>
  );
}