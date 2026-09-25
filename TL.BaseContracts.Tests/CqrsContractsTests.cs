using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TL.BaseContracts.CQRS;
using Xunit;

namespace TL.BaseContracts.Tests
{
    public class CqrsContractsTests
    {
        private sealed class CreateCustomerCommand : ICommand<Result<Guid>>
        {
            public Guid IdRequest { get; } = Guid.NewGuid();
            public string Name { get; set; } = string.Empty;
        }

        private sealed class CreateCustomerCommandHandler : ICommandHandler<CreateCustomerCommand, Result<Guid>>
        {
            public Task<Result<Guid>> HandleAsync(CreateCustomerCommand command, CancellationToken cancellationToken = default)
            {
                if (string.IsNullOrWhiteSpace(command.Name))
                {
                    return Task.FromResult(Result.Failure<Guid>(Error.Validation("Name.Empty", "Nome do cliente não pode ser vazio.")));
                }

                var generatedId = Guid.NewGuid();
                return Task.FromResult(Result.Success(generatedId));
            }
        }

        private sealed class DeactivateCustomerCommand : ICommand
        {
            public Guid IdRequest { get; } = Guid.NewGuid();
            public Guid CustomerId { get; set; }
        }

        private sealed class DeactivateCustomerCommandHandler : ICommandHandler<DeactivateCustomerCommand>
        {
            public Task<Result> HandleAsync(DeactivateCustomerCommand command, CancellationToken cancellationToken = default)
            {
                if (command.CustomerId == Guid.Empty)
                {
                    return Task.FromResult(Result.Failure(Error.Validation("CustomerId.Invalid", "Id do cliente é inválido.")));
                }

                return Task.FromResult(Result.Success());
            }
        }

        private sealed class GetCustomerByIdQuery : IQuery<Result<string>>
        {
            public Guid IdRequest { get; } = Guid.NewGuid();
            public Guid CustomerId { get; set; }
        }

        private sealed class GetCustomerByIdQueryHandler : IQueryHandler<GetCustomerByIdQuery, Result<string>>
        {
            public Task<Result<string>> HandleAsync(GetCustomerByIdQuery query, CancellationToken cancellationToken = default)
            {
                if (query.CustomerId == Guid.Empty)
                {
                    return Task.FromResult(Result.Failure<string>(Error.NotFound("Customer.NotFound", "Cliente não encontrado.")));
                }

                return Task.FromResult(Result.Success("Empresa Teste"));
            }
        }

        [Fact]
        public async Task Given_Command_With_Result_When_Handled_Should_Return_Success_Value()
        {
            var handler = new CreateCustomerCommandHandler();
            var command = new CreateCustomerCommand { Name = "Acme Corp" };

            var result = await handler.HandleAsync(command);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Given_Command_Without_Value_When_Handled_Should_Return_Result()
        {
            var handler = new DeactivateCustomerCommandHandler();
            var command = new DeactivateCustomerCommand { CustomerId = Guid.NewGuid() };

            var result = await handler.HandleAsync(command);

            result.IsSuccess.Should().BeTrue();
            result.Error.Should().Be(Error.None);
        }

        [Fact]
        public async Task Given_Query_When_Handled_Should_Return_Expected_Data()
        {
            var handler = new GetCustomerByIdQueryHandler();
            var query = new GetCustomerByIdQuery { CustomerId = Guid.NewGuid() };

            var result = await handler.HandleAsync(query);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be("Empresa Teste");
        }

        [Fact]
        public async Task Given_ServiceCollection_Should_Register_And_Resolve_Handlers_Directly()
        {
            var services = new ServiceCollection();
            services.AddTransient<ICommandHandler<CreateCustomerCommand, Result<Guid>>, CreateCustomerCommandHandler>();
            services.AddTransient<ICommandHandler<DeactivateCustomerCommand>, DeactivateCustomerCommandHandler>();
            services.AddTransient<IQueryHandler<GetCustomerByIdQuery, Result<string>>, GetCustomerByIdQueryHandler>();

            using var provider = services.BuildServiceProvider();

            var commandHandler = provider.GetService<ICommandHandler<CreateCustomerCommand, Result<Guid>>>();
            var voidCommandHandler = provider.GetService<ICommandHandler<DeactivateCustomerCommand>>();
            var queryHandler = provider.GetService<IQueryHandler<GetCustomerByIdQuery, Result<string>>>();

            commandHandler.Should().NotBeNull();
            voidCommandHandler.Should().NotBeNull();
            queryHandler.Should().NotBeNull();

            var result = await commandHandler!.HandleAsync(new CreateCustomerCommand { Name = "Direct Resolution" });
            result.IsSuccess.Should().BeTrue();
        }
    }
}

