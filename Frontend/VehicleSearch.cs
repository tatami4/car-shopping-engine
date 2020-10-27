﻿using DataTypes;
using System.Collections.Generic;

namespace Frontend
{
    // should this be static?
    public static class VehicleSearch
    {
        public static List<Car> searchVehicles(string brand,
                                               string model,
                                               FuelType? fuelType,
                                               ChassisType? vehicleType,
                                               SortingCriteria sorting,
                                               bool sortAscending,
                                               bool isUsed,
                                               bool isNew,
                                               uint lowerPriceRange,
                                               uint upperPriceRange,
                                               int lowerYearRange,
                                               int upperYearRange)
        {
            CarFilters filters = new CarFilters()
            {
                ChassisType = vehicleType,
                Brand = (brand == string.Empty) ? null : brand,
                Model = (model == string.Empty) ? null : model,
                PriceFrom = (lowerPriceRange == VehiclePropertyConstants.minVehiclePrice) ? default(uint?) : lowerPriceRange,
                PriceTo = (upperPriceRange == VehiclePropertyConstants.maxVehiclePrice) ? default(uint?) : upperPriceRange,
                Used = isUsed ? true : (isNew ? false : default(bool?)),
                YearFrom = (lowerYearRange == VehiclePropertyConstants.minVehicleManufactureYear) ? default(int?) : lowerYearRange,
                YearTo = (upperYearRange == VehiclePropertyConstants.maxVehicleManufactureYear) ? default(int?) : upperYearRange,
                FuelType = fuelType
            };

            // until required functionality is implemented in the frontend
            return null;
            // initiate search
        }
    }
}