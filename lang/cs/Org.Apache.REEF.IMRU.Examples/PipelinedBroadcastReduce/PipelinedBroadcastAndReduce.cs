﻿/**
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

using System.Globalization;
using Org.Apache.REEF.IMRU.API;
using Org.Apache.REEF.IO.PartitionedData.Random;
using Org.Apache.REEF.Tang.Annotations;
using Org.Apache.REEF.Tang.Implementations.Tang;
using Org.Apache.REEF.Tang.Util;
using Org.Apache.REEF.Wake.StreamingCodec.CommonStreamingCodecs;

namespace Org.Apache.REEF.IMRU.Examples.PipelinedBroadcastReduce
{
    /// <summary>
    /// IMRU program that performs broadcast and reduce
    /// </summary>
    public sealed class PipelinedBroadcastAndReduce
    {
        private readonly IIMRUClient<int[], int[], int[]> _imruClient;

        [Inject]
        private PipelinedBroadcastAndReduce(IIMRUClient<int[], int[], int[]> imruClient)
        {
            _imruClient = imruClient;
        }

        /// <summary>
        /// Runs the actual broadcast and reduce job
        /// </summary>
        public void Run(int numberofMappers, int chunkSize, int numIterations, int dim)
        {
            var updateFunctionConfig =
                TangFactory.GetTang().NewConfigurationBuilder(IMRUUpdateConfiguration<int[], int[], int[]>.ConfigurationModule
                    .Set(IMRUUpdateConfiguration<int[], int[], int[]>.UpdateFunction,
                        GenericType<BroadcastSenderReduceReceiverUpdateFunction>.Class).Build())
                    .BindNamedParameter(typeof (BroadcastReduceConfiguration.NumberOfIterations),
                        numIterations.ToString(CultureInfo.InvariantCulture))
                    .BindNamedParameter(typeof (BroadcastReduceConfiguration.Dimensions),
                        dim.ToString(CultureInfo.InvariantCulture))
                    .BindNamedParameter(typeof (BroadcastReduceConfiguration.NumWorkers),
                        numberofMappers.ToString(CultureInfo.InvariantCulture))
                    .Build();

            var dataConverterConfig1 =
                TangFactory.GetTang()
                    .NewConfigurationBuilder(IMRUPipelineDataConverterConfiguration<int[]>.ConfigurationModule
                        .Set(IMRUPipelineDataConverterConfiguration<int[]>.MapInputPiplelineDataConverter,
                            GenericType<PipelineIntDataConverter>.Class).Build())
                    .BindNamedParameter(typeof (BroadcastReduceConfiguration.ChunkSize),
                        chunkSize.ToString(CultureInfo.InvariantCulture))
                    .Build();

            var dataConverterConfig2 =
                TangFactory.GetTang()
                    .NewConfigurationBuilder(IMRUPipelineDataConverterConfiguration<int[]>.ConfigurationModule
                        .Set(IMRUPipelineDataConverterConfiguration<int[]>.MapInputPiplelineDataConverter,
                            GenericType<PipelineIntDataConverter>.Class).Build())
                    .BindNamedParameter(typeof (BroadcastReduceConfiguration.ChunkSize),
                        chunkSize.ToString(CultureInfo.InvariantCulture))
                    .Build();

            var results = _imruClient.Submit(
                new IMRUJobDefinitionBuilder()
                    .SetMapFunctionConfiguration(IMRUMapConfiguration<int[], int[]>.ConfigurationModule
                        .Set(IMRUMapConfiguration<int[], int[]>.MapFunction,
                            GenericType<BroadcastReceiverReduceSenderMapFunction>.Class)
                        .Build())
                    .SetUpdateFunctionConfiguration(updateFunctionConfig)
                    .SetMapInputCodecConfiguration(IMRUCodecConfiguration<int[]>.ConfigurationModule
                        .Set(IMRUCodecConfiguration<int[]>.Codec, GenericType<IntArrayStreamingCodec>.Class)
                        .Build())
                    .SetUpdateFunctionCodecsConfiguration(IMRUCodecConfiguration<int[]>.ConfigurationModule
                        .Set(IMRUCodecConfiguration<int[]>.Codec, GenericType<IntArrayStreamingCodec>.Class)
                        .Build())
                    .SetReduceFunctionConfiguration(IMRUReduceFunctionConfiguration<int[]>.ConfigurationModule
                        .Set(IMRUReduceFunctionConfiguration<int[]>.ReduceFunction,
                            GenericType<IntArraySumReduceFunction>.Class)
                        .Build())
                    .SetMapInputPipelineDataConverterConfiguration(dataConverterConfig1)
                    .SetMapOutputPipelineDataConverterConfiguration(dataConverterConfig2)
                    .SetPartitionedDatasetConfiguration(
                        RandomDataConfiguration.ConfigurationModule.Set(RandomDataConfiguration.NumberOfPartitions,
                            numberofMappers.ToString()).Build())
                    .SetJobName("BroadcastReduce")
                    .SetNumberOfMappers(numberofMappers)
                    .Build());
        }
    }
}