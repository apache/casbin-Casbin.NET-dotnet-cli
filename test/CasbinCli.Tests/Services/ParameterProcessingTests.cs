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

using CasbinCli.Services;  
using FluentAssertions;  
using System.Reflection;  
using Xunit;  
  
namespace CasbinCli.Tests.Services  
{  
    public class ParameterProcessingTests  
    {  
        private readonly EnforcementService _service;  
  
        public ParameterProcessingTests()  
        {  
            _service = new EnforcementService();  
        }  
  
        [Theory]  
        [InlineData(new[] { "alice", "data1", "read" }, 3)]  
        [InlineData(new[] { "{\"Age\":30}", "/data1", "read" }, 3)]  
        [InlineData(new[] { "bob", "{\"Owner\":\"bob\"}" }, 2)]  
        public void ProcessParameters_VariousInputs_ReturnsCorrectParameterCount(string[] input, int expectedCount)  
        {  
            // Act  
            var result = InvokeProcessParameters(input);  
  
            // Assert  
            result.Should().HaveCount(expectedCount);  
        }  
  
        [Fact]  
        public void ProcessParameters_JsonParameter_CreatesStructuredObject()  
        {  
            // Arrange  
            var input = new[] { "{\"Age\":30}", "/data1", "read" };  
  
            // Act  
            var result = InvokeProcessParameters(input);  
  
            // Assert  
            // 更新测试以验证动态类型而不是 Dictionary  
            result[0].Should().NotBeNull();  
              
            // 验证动态类型包含正确的字段和值  
            var dynamicObject = result[0];  
            var type = dynamicObject.GetType();  
              
            // 检查是否有 Age 字段（首字母大写）  
            var ageField = type.GetField("Age");  
            ageField.Should().NotBeNull("动态类型应该包含 Age 字段");  
              
            // 验证字段值  
            var ageValue = ageField?.GetValue(dynamicObject);  
            ageValue.Should().Be(30, "Age 字段的值应该是 30");  
        }  
  
        [Fact]  
        public void ProcessParameters_JsonParameterWithString_CreatesStructuredObject()  
        {  
            // Arrange  
            var input = new[] { "{\"Name\":\"alice\"}", "/data1", "read" };  
  
            // Act  
            var result = InvokeProcessParameters(input);  
  
            // Assert  
            result[0].Should().NotBeNull();  
              
            var dynamicObject = result[0];  
            var type = dynamicObject.GetType();  
              
            // 检查是否有 Name 字段（首字母大写）  
            var nameField = type.GetField("Name");  
            nameField.Should().NotBeNull("动态类型应该包含 Name 字段");  
              
            // 验证字段值  
            var nameValue = nameField?.GetValue(dynamicObject);  
            nameValue.Should().Be("alice", "Name 字段的值应该是 alice");  
        }  
  
        [Fact]  
        public void ProcessParameters_NonJsonParameter_ReturnsDirectValue()  
        {  
            // Arrange  
            var input = new[] { "alice", "data1", "read" };  
  
            // Act  
            var result = InvokeProcessParameters(input);  
  
            // Assert  
            result[0].Should().Be("alice");  
            result[1].Should().Be("data1");  
            result[2].Should().Be("read");  
        }  
  
        private object[] InvokeProcessParameters(string[] args)  
        {  
            var method = typeof(EnforcementService).GetMethod("ProcessParameters",   
                BindingFlags.NonPublic | BindingFlags.Instance);  
            return (object[])method?.Invoke(_service, new object[] { args }) ?? Array.Empty<object>();  
        }  
    }  
}