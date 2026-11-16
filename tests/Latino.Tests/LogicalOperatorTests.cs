using Xunit;
using Latino.Core.Types;

namespace Latino.Tests
{
    public class LogicalOperatorTests : TestBase
    {
        [Theory]
        [InlineData("verdadero ý verdadero", "cierto")]
        [InlineData("verdadero ý falso", "falso")]
        [InlineData("falso ý verdadero", "falso")]
        [InlineData("falso ý falso", "falso")]
        [InlineData("verdadero && verdadero", "cierto")]
        [InlineData("verdadero && falso", "falso")]
        [InlineData("falso && verdadero", "falso")]
        [InlineData("falso && falso", "falso")]
        public void TestAndOperator(string expression, string expected)
        {
            var result = Eval(expression);
            Assert.Equal(expected, result.ToString());
        }

        [Theory]
        [InlineData("verdadero ó verdadero", "cierto")]
        [InlineData("verdadero ó falso", "cierto")]
        [InlineData("falso ó verdadero", "cierto")]
        [InlineData("falso ó falso", "falso")]
        [InlineData("verdadero || verdadero", "cierto")]
        [InlineData("verdadero || falso", "cierto")]
        [InlineData("falso || verdadero", "cierto")]
        [InlineData("falso || falso", "falso")]
        public void TestOrOperator(string expression, string expected)
        {
            var result = Eval(expression);
            Assert.Equal(expected, result.ToString());
        }

        [Theory]
        [InlineData("no verdadero", "falso")]
        [InlineData("no falso", "cierto")]
        [InlineData("!verdadero", "falso")]
        [InlineData("!falso", "cierto")]
        public void TestNotOperator(string expression, string expected)
        {
            var result = Eval(expression);
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void TestCiertoPrincipal()
        {
            // Verifica que 'cierto' es el valor principal para true
            var result = Eval("cierto");
            Assert.True(((LatinoBoolean)result).Value);
            Assert.Equal("cierto", result.ToString()); // ToString() debe retornar "cierto"
        }

        [Fact]
        public void TestVerdaderoAlias()
        {
            // Verifica que 'verdadero' sigue funcionando como alias (compatibilidad)
            var result = Eval("verdadero");
            Assert.True(((LatinoBoolean)result).Value);
            Assert.Equal("cierto", result.ToString()); // ToString() debe retornar "cierto" (no "verdadero")
        }

        [Fact]
        public void TestFalsoPrincipal()
        {
            // Verifica que 'falso' es el valor principal para false
            var result = Eval("falso");
            Assert.False(((LatinoBoolean)result).Value);
            Assert.Equal("falso", result.ToString()); // ToString() debe retornar "falso"
        }

        [Fact]
        public void TestEspurioAlias()
        {
            // Verifica que 'espurio' funciona como alias culto
            var result = Eval("espurio");
            Assert.False(((LatinoBoolean)result).Value);
            Assert.Equal("falso", result.ToString()); // ToString() debe retornar "falso" (no "espurio")
        }

        [Theory]
        [InlineData("cierto ý cierto", "cierto")]
        [InlineData("cierto ý espurio", "falso")]
        [InlineData("espurio ý cierto", "falso")]
        [InlineData("espurio ý espurio", "falso")]
        public void TestAndOperatorConNuevosAlias(string expression, string expected)
        {
            var result = Eval(expression);
            Assert.Equal(expected, result.ToString());
        }

        [Theory]
        [InlineData("cierto ó cierto", "cierto")]
        [InlineData("cierto ó espurio", "cierto")]
        [InlineData("espurio ó cierto", "cierto")]
        [InlineData("espurio ó espurio", "falso")]
        public void TestOrOperatorConNuevosAlias(string expression, string expected)
        {
            var result = Eval(expression);
            Assert.Equal(expected, result.ToString());
        }

        [Theory]
        [InlineData("no cierto", "falso")]
        [InlineData("no espurio", "cierto")]
        [InlineData("!cierto", "falso")]
        [InlineData("!espurio", "cierto")]
        public void TestNotOperatorConNuevosAlias(string expression, string expected)
        {
            var result = Eval(expression);
            Assert.Equal(expected, result.ToString());
        }
    }
}
