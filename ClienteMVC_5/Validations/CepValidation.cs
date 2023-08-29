using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace ClienteMVC_5.Validations
{
    public class CepValidation : ValidationAttribute
    {
        public CepValidation()
        {

        }

        public override bool IsValid(object value)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return true;
            bool valido = ValidaCEP(value.ToString());
            return valido;
        }

        public bool ValidaCEP(string cep)

        {

            cep = cep.Replace(".", "");

            cep = cep.Replace("-", "");

            cep = cep.Replace(" ", "");



            Regex Rgx = new Regex(@"^\d{8}$");

            if (!Rgx.IsMatch(cep))

                return false;

            else

                return true;

        }
    }
}