namespace ConvertApp
{
    public class DynamicJsonObject
    {
        public string PropertyId { get; set; }
        public string PropertyName { get; set; }
        public string PropertyExtra { get; set; }

        public DynamicJsonObject(string propertyId, string propertyName, string propertyExtra)
        {
            PropertyId = propertyId;
            PropertyName = propertyName;
            PropertyExtra = propertyExtra;
        }

        public override string ToString()
        {
            return $"-- Id : {PropertyId} -- PropertyName : {PropertyName} -- PropertyExtra : {PropertyExtra} --";
        }
    }
}
