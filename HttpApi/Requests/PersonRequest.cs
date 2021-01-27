using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using FluentValidation;

namespace HttpApi.Requests
{
    public class PersonRequest
    {
        public string Name { get; set; }
        public DateTime Birthday { get; set; }
        public IEnumerable<int> Count { get; set; }
    }

    public class PersonRequestValidator : AbstractValidator<PersonRequest>
    {
        public PersonRequestValidator()
        {
            RuleFor(m => m.Name).NotEmpty();
        }
    }
}