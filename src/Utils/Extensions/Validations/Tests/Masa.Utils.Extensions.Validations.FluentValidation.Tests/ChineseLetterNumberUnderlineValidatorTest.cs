﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace Masa.Utils.Extensions.Validations.FluentValidation.Tests;

[TestClass]
public class ChineseLetterNumberUnderlineValidatorTest : ValidatorBaseTest
{
    public override string Message => "'Name' must be Chinese, numbers, letters or underscores.";

    [DataRow("团队123", true)]
    [DataRow("Masa团队", true)]
    [DataRow("masastack", true)]
    [DataRow("123", true)]
    [DataRow("masastack123", true)]
    [DataRow(".", false)]
    [DataRow("123.", false)]
    [DataRow("123_", true)]
    [DataTestMethod]
    public void TestChineseLetterNumberUnderline(string name, bool expectedResult)
    {
        var validator = new RegisterUserEventValidator();
        var result = validator.Validate(new RegisterUserEvent()
        {
            Name = name
        });
        Assert.AreEqual(expectedResult, result.IsValid);
        if (!expectedResult)
        {
            Assert.AreEqual(Message, result.Errors[0].ErrorMessage);
        }
    }

    public class RegisterUserEventValidator : AbstractValidator<RegisterUserEvent>
    {
        public RegisterUserEventValidator()
        {
            RuleFor(r => r.Name).ChineseLetterNumberUnderline();
        }
    }
}
