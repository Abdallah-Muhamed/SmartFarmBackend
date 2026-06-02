namespace Smart_Farm.Application.Exceptions;

/// <summary>Diagnosis failed — maps to HTTP 422 with no response body.</summary>
public sealed class DiagnosisUnprocessableException : Exception
{
    public DiagnosisUnprocessableException() : base() { }

    public DiagnosisUnprocessableException(Exception innerException) : base(null, innerException) { }
}
