namespace CSharpToTypeScript.Server.DTOs
{
    public class Output
    {
        public string ConvertedCode { get; set; }
        public string ConvertedFileName { get; set; }
        public System.Collections.Generic.List<ConvertedFile> ConvertedFiles { get; set; }
        public bool Succeeded { get; set; }
        public string ErrorMessage { get; set; }
    }
}