// Copyright 2025 The casbin Authors. All Rights Reserved.  
//  
// Licensed under the Apache License, Version 2.0 (the "License");  
// you may not use this file except in compliance with the License.  
// You may obtain a copy of the License at  
//  
//      http://www.apache.org/licenses/LICENSE-2.0  
//  
// Unless required by applicable law or agreed to in writing, software  
// distributed under the License is distributed on an "AS IS" BASIS,  
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  
// See the License for the specific language governing permissions and  
// limitations under the License.

using CasbinCli.Models;  
using CasbinCli.Services;  
using FluentAssertions;  
using Xunit;  
  
namespace CasbinCli.Tests.Services  
{  
    public class EnforcementServiceTests  
    {  
        private readonly EnforcementService _service;  
        private readonly string _testDataPath;  
  
        public EnforcementServiceTests()  
        {  
            _service = new EnforcementService();  
            _testDataPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "test");  
        }  
  
        [Fact]  
        public async Task ExecuteEnforceAsync_BasicModel_AliceCanReadData1_ReturnsTrue()  
        {  
            // Arrange  
            var modelPath = Path.Combine(_testDataPath, "basic_model.conf");  
            var policyPath = Path.Combine(_testDataPath, "basic_policy.csv");  
            var args = new[] { "alice", "data1", "read" };  
  
            // Act  
            var result = await _service.ExecuteEnforceAsync(modelPath, policyPath, args, false);  
  
            // Assert  
            result.Allow.Should().BeTrue();  
            result.Explain.Should().BeEmpty();  
        }  
  
        [Fact]  
        public async Task ExecuteEnforceAsync_BasicModel_AliceCannotWriteData1_ReturnsFalse()  
        {  
            // Arrange  
            var modelPath = Path.Combine(_testDataPath, "basic_model.conf");  
            var policyPath = Path.Combine(_testDataPath, "basic_policy.csv");  
            var args = new[] { "alice", "data1", "write" };  
  
            // Act  
            var result = await _service.ExecuteEnforceAsync(modelPath, policyPath, args, false);  
  
            // Assert  
            result.Allow.Should().BeFalse();  
            result.Explain.Should().BeEmpty();  
        }  
  
        [Fact]  
        public async Task ExecuteEnforceAsync_BasicModelWithExplanation_AliceCanReadData1_ReturnsExplanation()  
        {  
            // Arrange  
            var modelPath = Path.Combine(_testDataPath, "basic_model.conf");  
            var policyPath = Path.Combine(_testDataPath, "basic_policy.csv");  
            var args = new[] { "alice", "data1", "read" };  
  
            // Act  
            var result = await _service.ExecuteEnforceAsync(modelPath, policyPath, args, true);  
  
            // Assert  
            result.Allow.Should().BeTrue();  
            result.Explain.Should().NotBeEmpty();  
        }  
  
        [Fact]  
        public async Task ExecuteEnforceAsync_RbacWithDomains_AliceCanReadData1InDomain1_ReturnsTrue()  
        {  
            // Arrange  
            var modelPath = Path.Combine(_testDataPath, "rbac_with_domains_model.conf");  
            var policyPath = Path.Combine(_testDataPath, "rbac_with_domains_policy.csv");  
            var args = new[] { "alice", "domain1", "data1", "read" };  
  
            // Act  
            var result = await _service.ExecuteEnforceAsync(modelPath, policyPath, args, true);  
  
            // Assert  
            result.Allow.Should().BeTrue();  
            result.Explain.Should().NotBeEmpty();  
        }  
  
        [Fact]  
        public async Task ExecuteEnforceAsync_AbacWithJsonParameter_AgeOver18CanRead_ReturnsTrue()  
        {  
            // Arrange  
            var modelPath = Path.Combine(_testDataPath, "abac_rule_model.conf");  
            var policyPath = Path.Combine(_testDataPath, "abac_rule_policy.csv");  
            var args = new[] { "{\"Age\":30}", "/data1", "read" };  
  
            // Act  
            var result = await _service.ExecuteEnforceAsync(modelPath, policyPath, args, true);  
  
            // Assert  
            result.Allow.Should().BeTrue();  
        }  
  
        [Fact]  
        public async Task ExecuteEnforceAsync_AbacWithJsonParameter_AgeUnder18CannotRead_ReturnsFalse()  
        {  
            // Arrange  
            var modelPath = Path.Combine(_testDataPath, "abac_rule_model.conf");  
            var policyPath = Path.Combine(_testDataPath, "abac_rule_policy.csv");  
            var args = new[] { "{\"Age\":15}", "/data1", "read" };  
  
            // Act  
            var result = await _service.ExecuteEnforceAsync(modelPath, policyPath, args, true);  
  
            // Assert  
            result.Allow.Should().BeFalse();  
        }  
  
        [Fact]  
        public async Task ExecuteEnforceAsync_InvalidModelFile_ReturnsErrorResponse()  
        {  
            // Arrange  
            var modelPath = "nonexistent_model.conf";  
            var policyPath = Path.Combine(_testDataPath, "basic_policy.csv");  
            var args = new[] { "alice", "data1", "read" };  
  
            // Act  
            var result = await _service.ExecuteEnforceAsync(modelPath, policyPath, args, false);  
  
            // Assert  
            result.Allow.Should().BeFalse();  
            result.Explain.Should().BeEmpty();  
        }  
  
        [Fact]  
        public async Task ExecuteEnforceAsync_InvalidPolicyFile_ReturnsErrorResponse()  
        {  
            // Arrange  
            var modelPath = Path.Combine(_testDataPath, "basic_model.conf");  
            var policyPath = "nonexistent_policy.csv";  
            var args = new[] { "alice", "data1", "read" };  
  
            // Act  
            var result = await _service.ExecuteEnforceAsync(modelPath, policyPath, args, false);  
  
            // Assert  
            result.Allow.Should().BeFalse();  
            result.Explain.Should().BeEmpty();  
        }  
  
        [Fact]  
        public async Task ExecuteEnforceAsync_NoParameters_ReturnsErrorResponse()  
        {  
            // Arrange  
            var modelPath = Path.Combine(_testDataPath, "basic_model.conf");  
            var policyPath = Path.Combine(_testDataPath, "basic_policy.csv");  
            var args = Array.Empty<string>();  
  
            // Act  
            var result = await _service.ExecuteEnforceAsync(modelPath, policyPath, args, false);  
  
            // Assert  
            result.Allow.Should().BeFalse();  
            result.Explain.Should().BeEmpty();  
        }  
    }  
}