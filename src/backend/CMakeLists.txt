cmake_minimum_required(VERSION 3.16)

project(liv_backend VERSION 0.0.1 LANGUAGES CXX)

if(UNITTESTS_COVERAGE)
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fprofile-arcs -ftest-coverage")
    set(EXTRA_LIBS_COVERAGE "gcov")
else()
    set(EXTRA_LIBS_COVERAGE "")
endif()

add_library(liv_backend_lib STATIC)

target_sources(liv_backend_lib
    PRIVATE
        ImgProvider.cpp
        ImgProvider.h
        BackendManager.cpp
        BackendManager.h
        IFileSystemWalker.h
        FileSystemWalker.cpp
        FileSystemWalker.h
        IDir.h
        DirWrapper.cpp
        DirWrapper.h
        IFileSystemClassesFactory.h
        FileSystemWrapperFactory.cpp
        FileSystemWrapperFactory.h
)

target_include_directories(liv_backend_lib
    PUBLIC
        "${CMAKE_CURRENT_SOURCE_DIR}"
)

target_link_libraries(liv_backend_lib
    PRIVATE
        Qt6::Quick
        ${EXTRA_LIBS_COVERAGE})

# **** unittests ****
add_executable(${PROJECT_NAME}-test)

target_sources(${PROJECT_NAME}-test
    PRIVATE
        unittests.cpp
        ImgProvider-test.cpp
        FileSystemWalker-test.cpp
)

target_link_libraries(${PROJECT_NAME}-test
    PRIVATE
        liv_backend_lib
        Qt6::Quick
        GTest::gmock
        GTest::gtest_main
        ${EXTRA_LIBS_COVERAGE}
)

configure_file("${CMAKE_SOURCE_DIR}/data/testimages/lenna.jpg" "${CMAKE_CURRENT_BINARY_DIR}/lenna.jpg" COPYONLY)
add_test(${PROJECT_NAME}-gtests ${PROJECT_NAME}-test -platform offscreen "${UNITTESTS_EXTRA_ARGS}")
