cmake_minimum_required(VERSION 3.16)

project(lightweightimageviewer VERSION 0.0.1 LANGUAGES CXX)

set(CMAKE_AUTOMOC ON)
set(CMAKE_AUTORCC ON)
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(RUN_CLANG_TIDY OFF CACHE BOOL "if ON clang-tidy is executed as part of the build process")
set(UNITTESTS_EXTRA_ARGS "" CACHE STRING "additional command line arguments for the unittests execution")
set(UNITTESTS_COVERAGE OFF CACHE BOOL "if ON code coverage is evaluated, otherwise not")

find_package(Qt6 6.2 COMPONENTS Quick QuickTest REQUIRED)

enable_testing()

add_subdirectory(src)
