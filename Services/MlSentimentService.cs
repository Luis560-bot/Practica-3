using Microsoft.ML;
using Practica3.Models;

namespace Practica3.Services;

public interface IMlSentimentService
{
    string PredictSentiment(string text);
}

public class MlSentimentService : IMlSentimentService
{
    private readonly MLContext _mlContext;
    private readonly ITransformer _model;

    public MlSentimentService()
    {
        // Initialize ML.NET context with a fixed seed for reproducible results
        _mlContext = new MLContext(seed: 1);

        // High-quality training dataset in Spanish for sentiment analysis
        var trainingData = new List<SentimentData>
        {
            // Positive Sentiments
            new SentimentData { SentimentText = "La tarea fue completada correctamente y el sistema funciona bien", Label = true },
            new SentimentData { SentimentText = "Excelente trabajo, funciona perfectamente", Label = true },
            new SentimentData { SentimentText = "Me encanta este sistema, es muy rápido", Label = true },
            new SentimentData { SentimentText = "El resultado es maravilloso y estoy muy feliz", Label = true },
            new SentimentData { SentimentText = "Es una gran solución para el proyecto", Label = true },
            new SentimentData { SentimentText = "El código está limpio y ordenado", Label = true },
            new SentimentData { SentimentText = "La interfaz es intuitiva y fácil de usar", Label = true },
            new SentimentData { SentimentText = "Es fantástico, funciona de maravilla", Label = true },
            new SentimentData { SentimentText = "Todo marcha de forma ideal", Label = true },
            new SentimentData { SentimentText = "Cumple perfectamente con todos los requisitos", Label = true },
            new SentimentData { SentimentText = "Es una excelente herramienta, muy útil", Label = true },
            new SentimentData { SentimentText = "Me gusta mucho cómo responde la aplicación", Label = true },
            new SentimentData { SentimentText = "El rendimiento es sobresaliente y ágil", Label = true },
            new SentimentData { SentimentText = "Una experiencia de usuario inmejorable", Label = true },
            new SentimentData { SentimentText = "Ha superado todas mis expectativas", Label = true },
            new SentimentData { SentimentText = "Muy bien hecho, sigan así", Label = true },
            new SentimentData { SentimentText = "Me siento muy satisfecho con los resultados obtenidos", Label = true },
            new SentimentData { SentimentText = "Increíble rapidez y gran atención a los detalles", Label = true },
            new SentimentData { SentimentText = "Es muy fácil configurar y empezar a usar", Label = true },
            new SentimentData { SentimentText = "Me ayudó muchísimo a completar mi trabajo a tiempo", Label = true },

            // Negative Sentiments
            new SentimentData { SentimentText = "No funciona, da muchos errores", Label = false },
            new SentimentData { SentimentText = "El sistema es sumamente lento y se cae a cada rato", Label = false },
            new SentimentData { SentimentText = "Es el peor programa que he visto", Label = false },
            new SentimentData { SentimentText = "El diseño es horrible y es muy difícil de entender", Label = false },
            new SentimentData { SentimentText = "No me gusta este servicio", Label = false },
            new SentimentData { SentimentText = "Falla al compilar y no responde las peticiones", Label = false },
            new SentimentData { SentimentText = "Tarda demasiado tiempo en cargar las páginas", Label = false },
            new SentimentData { SentimentText = "Está lleno de bugs y problemas técnicos", Label = false },
            new SentimentData { SentimentText = "Es una pérdida de tiempo total", Label = false },
            new SentimentData { SentimentText = "No cumple con lo que promete", Label = false },
            new SentimentData { SentimentText = "El soporte técnico es pésimo y no ayuda", Label = false },
            new SentimentData { SentimentText = "Es confuso, frustrante y lento", Label = false },
            new SentimentData { SentimentText = "La aplicación es inestable y se cierra sola", Label = false },
            new SentimentData { SentimentText = "No recomiendo usar este sistema para nada", Label = false },
            new SentimentData { SentimentText = "Tiene un rendimiento muy pobre", Label = false },
            new SentimentData { SentimentText = "Es un desastre total, nada funciona bien", Label = false },
            new SentimentData { SentimentText = "Tengo muchos problemas al intentar usarlo", Label = false },
            new SentimentData { SentimentText = "Muy mala experiencia de usuario, horrible", Label = false },
            new SentimentData { SentimentText = "No sirve de nada, es inútil", Label = false },
            new SentimentData { SentimentText = "El código es un caos completo", Label = false }
        };

        // Convert the enumerable list to an IDataView
        var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

        // Build the training pipeline:
        // 1. Featurize the sentiment text to numeric values (Features)
        // 2. Append the SDCA Logistic Regression binary classification trainer
        var pipeline = _mlContext.Transforms.Text.FeaturizeText(
            outputColumnName: "Features",
            inputColumnName: nameof(SentimentData.SentimentText)
        )
        .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
            labelColumnName: "Label",
            featureColumnName: "Features"
        ));

        // Train the machine learning model
        _model = pipeline.Fit(dataView);
    }

    public string PredictSentiment(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "Neutro";

        // Create a prediction engine
        var predictionEngine = _mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(_model);

        var sampleData = new SentimentData { SentimentText = text };
        var prediction = predictionEngine.Predict(sampleData);

        return prediction.Prediction ? "Positivo" : "Negativo";
    }
}
