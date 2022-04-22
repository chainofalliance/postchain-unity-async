using System.Collections.Generic;

namespace Chromia.Postchain.Ft3
{
    public interface IAuthdescriptorRule
    {
        object[] ToGTV();
    };

    public class Rules
    {
        public static RuleVariable BlockHeight()
        {
            return new RuleVariable("block_height");
        }

        public static RuleVariable BlockTime()
        {
            return new RuleVariable("block_time");
        }

        public static RuleVariable OperationCount()
        {
            return new RuleVariable("op_count");
        }
    }

    public class RuleVariable
    {
        private string Variable;

        public RuleVariable(string variable)
        {
            this.Variable = variable;
        }

        public RuleExpression LessThan(long value)
        {
            return this.Expression("lt", value);
        }

        public RuleExpression LessOrEqual(long value)
        {
            return this.Expression("le", value);
        }

        public RuleExpression Equal(long value)
        {
            return this.Expression("eq", value);
        }

        public RuleExpression GreaterThan(long value)
        {
            return this.Expression("gt", value);
        }

        public RuleExpression GreaterOrEqual(long value)
        {
            return this.Expression("ge", value);
        }

        private RuleExpression Expression(string op, long value)
        {
            return new RuleExpression(this.Variable, op, value);
        }
    }

    public class RuleExpression : IAuthdescriptorRule
    {
        public readonly string Name;
        public readonly string Operator;
        public readonly long Value;

        public RuleExpression(string name, string op, long value)
        {
            Name = name;
            Operator = op;
            Value = value;
        }

        public RuleCompositeExpressionOperator And()
        {
            return new RuleCompositeExpressionOperator(this, "and");
        }

        public object[] ToGTV()
        {
            var gtv = new object[] {
                Name,
                Operator,
                Value
            };

            return gtv;
        }
    }

    public class RuleCompositeExpressionOperator
    {
        public readonly IAuthdescriptorRule Expression;
        public readonly string Operator;

        public RuleCompositeExpressionOperator(IAuthdescriptorRule expression, string op)
        {
            Expression = expression;
            Operator = op;
        }

        public RuleCompositeExpressionVariable BlockHeight()
        {
            return new RuleCompositeExpressionVariable(this.Expression, "block_height", this.Operator);
        }

        public RuleCompositeExpressionVariable BlockTime()
        {
            return new RuleCompositeExpressionVariable(this.Expression, "block_time", this.Operator);
        }

        public RuleCompositeExpressionVariable OperationCount()
        {
            return new RuleCompositeExpressionVariable(this.Expression, "op_count", this.Operator);
        }
    }

    public class RuleCompositeExpressionVariable
    {
        public readonly IAuthdescriptorRule Expression;
        public readonly string Name;
        public readonly string Operator;

        public RuleCompositeExpressionVariable(IAuthdescriptorRule expression, string name, string op)
        {
            Expression = expression;
            Name = name;
            Operator = op;
        }

        public RuleCompositeExpression LessThan(long value)
        {
            return this.CompositeExpression("lt", value);
        }

        public RuleCompositeExpression LessOrEqual(long value)
        {
            return this.CompositeExpression("le", value);
        }

        public RuleCompositeExpression Equal(long value)
        {
            return this.CompositeExpression("eq", value);
        }

        public RuleCompositeExpression GreaterThan(long value)
        {
            return this.CompositeExpression("gt", value);
        }

        public RuleCompositeExpression GreaterOrEqual(long value)
        {
            return this.CompositeExpression("ge", value);
        }

        private RuleCompositeExpression CompositeExpression(string op, long value)
        {
            return new RuleCompositeExpression(this.Operator, this.Expression, new RuleExpression(this.Name, op, value));
        }
    }

    public class RuleCompositeExpression : IAuthdescriptorRule
    {
        public readonly string Operator;
        public readonly IAuthdescriptorRule Left;
        public readonly RuleExpression Right;

        public RuleCompositeExpression(string op, IAuthdescriptorRule left, RuleExpression right)
        {
            Operator = op;
            Left = left;
            Right = right;
        }

        public RuleCompositeExpressionOperator And()
        {
            return new RuleCompositeExpressionOperator(this, "and");
        }

        public object[] ToGTV()
        {
            var gtv = new object[] {
                Left.ToGTV(),
                Operator,
                Right.ToGTV()
            };

            return gtv;
        }
    }
}
