using System;
using System.Text;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Utils
{
    public static class IndexNameConverter
    {
        public static string FillIndexNamePlaceholder(string aliasFormat, string index)
        {
            const string placeholder = "{index}"; //NOTE '{index}' also used by ES while index created using template with alias. do not change
            return aliasFormat.Replace(placeholder, index);
        }

        public static string ConvertToDateTimeFormat(string s)
        {
            var sb = new StringBuilder();
            var count = 0;
            for(var index = 0; index < s.Length; index++)
            {
                var c = s[index];
                if(c == '{') ++count;
                if(c == '}') --count;
                switch(count)
                {
                case 1:
                    if(c != '{')
                        sb.Append(c);
                    break;
                case 0:
                    if(c != '}')
                    {
                        sb.Append('\\');
                        sb.Append(c);
                    }
                    break;
                default:
                    if(count > 1)
                        throw new NotSupportedException(string.Format("Inner '{{}}' not supported. position {0}", index));
                    if(count < 0)
                        throw new NotSupportedException(string.Format("'{{}}' not balanced. Unexpected '{{'  at position {0}", index));
                    break;
                }
            }
            if(count != 0)
                throw new NotSupportedException(string.Format("'{{}}' not balanced. Missing '}}'"));
            return sb.ToString();
        }
    }
}