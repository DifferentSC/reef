/**
 * Copyright (C) 2014 Microsoft Corporation
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *         http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
package com.microsoft.reef.runtime.common.driver.resourcemanager;

import com.microsoft.reef.proto.ReefServiceProtos;
import com.microsoft.reef.runtime.common.driver.DriverShutdownManager;
import com.microsoft.wake.EventHandler;

import javax.inject.Inject;

/**
 * Informs the client and then shuts down the driver forcefully in case of Resource Manager errors.
 */
public final class ResourceManagerErrorHandler implements EventHandler<ReefServiceProtos.RuntimeErrorProto> {


  private final DriverShutdownManager driverShutdownManager;

  @Inject
  ResourceManagerErrorHandler(final DriverShutdownManager driverShutdownManager) {
    this.driverShutdownManager = driverShutdownManager;
  }

  @Override
  public synchronized void onNext(final ReefServiceProtos.RuntimeErrorProto runtimeErrorProto) {
    this.driverShutdownManager.onError(new Exception("Resource Manager failure"));
  }
}
