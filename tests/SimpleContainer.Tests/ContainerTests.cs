using FluentAssertions;
using System;
using Xunit;

namespace SimpleContainer.Tests
{
	public class ContainerTests
	{
		[Fact]
		public static void WhenNoServicesAreRegisteredThenResolvingServiceReturnsNull()
		{
			Action act = () => new Container().Resolve<object>();
			act.ShouldNotThrow<Exception>();
		}

		internal interface IFoo { }
		internal sealed class Foo : IFoo { }

		[Fact]
		public static void WhenServiceTypeIsRegisteredThenResolvesInstance()
		{
			var container = new Container();
			container.Register<IFoo, Foo>();
			container.Resolve<IFoo>().Should().NotBeNull();
		}

		[Fact]
		public static void WhenServiceIsNotRegisteredThenCanResolveSelfBinding()
		{
			new Container().Resolve<Foo>()
				.Should().NotBeNull();
		}

		public class Bar { }

		[Fact]
		public static void WhenServiceIsNotAssignableFromImplementationThenRegistrationShouldThrowException()
		{
			var container = new Container();
			Action act = () => container.Register(typeof(IFoo), typeof(Bar));
			act.ShouldThrow<Exception>();
		}

		// TODO : Should the container be able to resolve itself for IContainer
		// TODO : Should the container be able to resolve itself for IServiceProvider
		// TODO : Should the container be able to resolve itself on Self Bind (Container)
	}
}
