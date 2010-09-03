using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using Microsoft.CSharp.RuntimeBinder;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;



namespace DynamicCheckPropertyExistence
{
    class Program
    {        
        static void Main(string[] args)
        {
            dynamic testDynamicObject = new ExpandoObject();
            testDynamicObject.Name = "Testovaci vlastnost";
            Console.WriteLine(HasPropertyNaive(testDynamicObject, "Name"));
            Console.WriteLine(HasPropertyNaive(testDynamicObject, "Id"));            
            Console.ReadLine();

            Console.WriteLine(HasProperty(testDynamicObject, "Name"));
            Console.WriteLine(HasProperty(testDynamicObject, "Id"));            
            Console.ReadLine();
        }

        private static bool HasPropertyNaive(IDynamicMetaObjectProvider dynamicProvider, string name)
        {
            try
            {
                var callSite =
                                CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, name, typeof(Program),
                                                         new[]
                                                                 {
                                                                     CSharpArgumentInfo.Create(
                                                                         CSharpArgumentInfoFlags.None, null)
                                                                 }));
                callSite.Target(callSite, dynamicProvider);
                return true;
            }
            catch (RuntimeBinderException)
            {

                return false;
            }

        }

        private static bool HasProperty(IDynamicMetaObjectProvider dynamicProvider, string name)
        {



            var defaultBinder = Binder.GetMember(CSharpBinderFlags.None, name, typeof(Program),
                             new[]
                                     {
                                         CSharpArgumentInfo.Create(
                                         CSharpArgumentInfoFlags.None, null)
                                     }) as GetMemberBinder;


            var callSite = CallSite<Func<CallSite, object, object>>.Create(new NoThrowGetBinderMember(name, false, defaultBinder));


            var result = callSite.Target(callSite, dynamicProvider);

            if (Object.ReferenceEquals(result, NoThrowExpressionVisitor.DUMMY_RESULT))
            {
                return false;
            }

            return true;

        }

      

    }

    class NoThrowGetBinderMember : GetMemberBinder
    {
        private GetMemberBinder m_innerBinder;        
        
        public NoThrowGetBinderMember(string name, bool ignoreCase, GetMemberBinder innerBinder) : base(name, ignoreCase)
        {
            m_innerBinder = innerBinder;            
        }
        
        public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {


            var retMetaObject = m_innerBinder.Bind(target, new DynamicMetaObject[] {});            
            
            var noThrowVisitor = new NoThrowExpressionVisitor();

            var resultExpression = noThrowVisitor.Visit(retMetaObject.Expression);
            Console.WriteLine(retMetaObject.Expression.ToString());            

            var finalMetaObject = new DynamicMetaObject(resultExpression, retMetaObject.Restrictions);
            return finalMetaObject;

        }
        
    }

    class NoThrowExpressionVisitor : ExpressionVisitor
    {        
        public static readonly object DUMMY_RESULT = new DummyBindingResult();
        
        public NoThrowExpressionVisitor()
        {
            
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            
            if (node.IfFalse.NodeType != ExpressionType.Throw)
            {
                return base.VisitConditional(node);
            }

            Console.WriteLine(node.ToString());
            
            Expression<Func<Object>> dummyFalseResult = () => DUMMY_RESULT;
            var invokeDummyFalseResult = Expression.Invoke(dummyFalseResult, null);                                    
            return Expression.Condition(node.Test, node.IfTrue, invokeDummyFalseResult);
        }

        private class DummyBindingResult {}       
    }
}





